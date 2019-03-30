using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

using ProtoBuf.ServiceModel;

namespace VirtualRobotWrapperLib.Wcf
{
    public abstract class WcfServiceProcessRunner<T> : WcfServiceProcessRunnerBase where T : class, IRemoteService
    {
        #region Events

        public event EventHandler Started;

        #endregion

        #region Fields

        private readonly ManualResetEvent _serverStarted = new ManualResetEvent(false);
        private readonly object _lock = new object();
        private readonly string _processPath;

        private volatile bool _startCalled;
        private volatile bool _running;
        private volatile bool _disposed;
        private volatile bool _noRestart;

        private int _connectionErrorCounter;
        private Thread _threadServer;
        private ChannelFactory<T> _pipeFactory;

        #endregion

        #region Instance

        protected WcfServiceProcessRunner()
        {
            _processPath = ProcessName;

            Task.Factory.StartNew(
                Start,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }

        ~WcfServiceProcessRunner()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool ExitRequested
        {
            get
            {
                WaitForStart();

                return Pipe.ExitRequested;
            }
        }

        public bool IsRemote => false;

        protected T Pipe { get; private set; }

        // ReSharper disable once SuspiciousTypeConversion.Global
        protected IChannel Channel => (IChannel)Pipe;

        protected abstract string ProcessName { get; }

        protected abstract string ServerName { get; }

        protected abstract string ProcessArguments { get; }

        #endregion

        #region Public methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Running()
        {
            WaitForStart();

            return Pipe.Running();
        }

        public void Exit()
        {
            WaitForStart();

            Dispose();
        }

        #endregion

        #region Protected methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WaitForStart()
        {
            if (_running)
                return;

            while (!_startCalled)
                Thread.Sleep(10);

            lock (_lock) { }
        }

        protected void Restart([CallerMemberName] string caller = null)
        {
            if (_noRestart)
                return;

            string serverName;
            lock (_lock)
            {
                serverName = ServerName;

                if (!_running)
                {
                    WaitForStart();

                    return;
                }

                _running = false;
                _startCalled = false;
            }

            Stop();

            Task.Factory.StartNew(
                Start,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);

            WaitForStart();
        }

        #endregion

        #region Private methods

        // ReSharper disable once UnusedParameter.Local
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            Task.Factory.StartNew(
                () =>
                {
                    WaitForStart();

                    Stop();
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }

        private void Start()
        {
            lock (_lock)
            {
                var restart = false;

                _threadServer = new Thread(RunServer);
                _threadServer.Start(null);

                var endPoint = new EndpointAddress($"net.pipe://localhost/{ServerName}");

                var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
                {
                    TransferMode = TransferMode.Buffered,
                    ReceiveTimeout = TimeSpan.MaxValue,
                    MaxBufferPoolSize = int.MaxValue,
                    MaxBufferSize = int.MaxValue,
                    MaxReceivedMessageSize = int.MaxValue,
                    ReaderQuotas =
                    {
                        MaxArrayLength = int.MaxValue,
                        MaxBytesPerRead = int.MaxValue,
                        MaxDepth = int.MaxValue,
                        MaxNameTableCharCount = int.MaxValue,
                        MaxStringContentLength = int.MaxValue
                    }
                };

                _pipeFactory = new ChannelFactory<T>(binding, endPoint);
                _pipeFactory.Endpoint.EndpointBehaviors.Add(new ProtoEndpointBehavior());

                if (!DebugWcf)
                {
                    var visualStudioDebug =
                        _pipeFactory.Endpoint.EndpointBehaviors.FirstOrDefault(
                            b => b.GetType().FullName == "Microsoft.VisualStudio.Diagnostics.ServiceModelSink.Behavior");
                    if (visualStudioDebug != null)
                        _pipeFactory.Endpoint.EndpointBehaviors.Remove(visualStudioDebug);
                }

                _serverStarted.WaitOne();

                Exception startException = null;
                try
                {
                    Pipe = _pipeFactory.CreateChannel();

                    // ReSharper disable once SuspiciousTypeConversion.Global
                    ((IContextChannel)Pipe).OperationTimeout = TimeSpan.MaxValue;

                    try
                    {
                        Process.GetCurrentProcess().ProcessorAffinity = new IntPtr((1 << Environment.ProcessorCount) - 1);
                    }
                    catch
                    {
                        // ignored
                    }

                    var running = false;
                    while (!running && _threadServer.IsAlive)
                    {
                        try
                        {
                            if (Channel.State == CommunicationState.Faulted ||
                                Channel.State == CommunicationState.Closing ||
                                Channel.State == CommunicationState.Closed)
                            {
                                restart = true;
                                break;
                            }

                            running = Pipe.Running();

                            startException = null;
                        }
                        catch (Exception ex)
                        {
                            startException = ex;
                        }
                    }

                    if (!running)
                        restart = true;
                }
                finally
                {
                    if (!restart)
                    {
                        _startCalled = true;

                        try
                        {
                            _noRestart = true;

                            Started?.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                            // ignored
                        }
                        finally
                        {
                            _noRestart = false;
                        }

                        _running = true;
                        _connectionErrorCounter = 0;
                    }
                }

                if (!restart || _connectionErrorCounter >= 10)
                    return;

                _connectionErrorCounter++;

                Stop();

                Task.Factory.StartNew(
                    Start,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default);
            }
        }

        private void Stop()
        {
            lock (_lock)
            {
                _startCalled = false;
                _serverStarted.Reset();

                var channel = Channel;

                if (Pipe != null && _running)
                {
                    try
                    {
                        Pipe.Exit();
                        (Pipe as IDisposable)?.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }

                    Pipe = null;
                }

                try
                {
                    if (channel != null)
                    {
                        try
                        {
                            channel.Close(TimeSpan.FromSeconds(10));
                        }
                        catch (CommunicationException)
                        {
                            channel.Abort();
                        }
                        catch (TimeoutException)
                        {
                            channel.Abort();
                        }
                        catch (Exception)
                        {
                            channel.Abort();
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if (_pipeFactory != null)
                    {
                        try
                        {
                            _pipeFactory.Close(TimeSpan.FromSeconds(10));
                        }
                        catch (CommunicationException)
                        {
                            _pipeFactory.Abort();
                        }
                        catch (TimeoutException)
                        {
                            _pipeFactory.Abort();
                        }
                        catch (Exception)
                        {
                            _pipeFactory.Abort();
                        }
                    }

                    _pipeFactory = null;
                }
                catch
                {
                    // ignored
                }

                if (_threadServer == null)
                    return;

                if (!_threadServer.Join(10 * 1000))
                    _threadServer.Abort();
                _threadServer = null;
            }
        }

        private void RunServer(object state)
        {
            var debugServer = DebugServer && Debugger.IsAttached ? " DEBUG" : "";
            var debugWcf = DebugWcf ? " DEBUGWCF" : "";

            var info = new ProcessStartInfo
            {
                FileName = _processPath,
                Arguments = $"\"{ServerName}\" {ProcessArguments}{debugServer}{debugWcf}",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            ProcessRunner.Run(ServerName, info, _serverStarted, "ready");
        }

        #endregion
    }
}