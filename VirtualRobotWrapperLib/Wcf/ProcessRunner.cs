using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualRobotWrapperLib.Wcf
{
    public class ProcessRunner : IDisposable
    {
        #region Fields

        private Process _process;

        #endregion

        #region Instance

        private ProcessRunner() { }

        ~ProcessRunner()
        {
            Dispose(false);
        }

        #endregion

        #region Public methods

        public static void Run(string name, ProcessStartInfo info, ManualResetEvent started, string readyText)
        {
            var processRunner = new ProcessRunner();
            processRunner.DoRun(name, info, started, readyText);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private methods

#if DEBUG
        private static bool IsNullStream(TextWriter writer)
        {
            if (writer == null)
                return false;

            var outField = writer.GetType().GetField("_out", BindingFlags.Instance | BindingFlags.NonPublic);
            if (outField == null)
                return false;

            var streamWriter = outField.GetValue(writer) as StreamWriter;
            return streamWriter != null && ReferenceEquals(StreamWriter.Null, streamWriter);
        }
#endif

        private static void ReadOutput(
            string name,
            ManualResetEvent started,
            string readyText,
            TextReader reader,
            TextWriter[] writers)
        {
            try
            {
                var threadSafeTextReader = TextReader.Synchronized(reader);

                string line;
                while ((line = threadSafeTextReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (readyText == line)
                        started?.Set();

                    if (writers != null)
                    {
                        foreach (var writer in writers)
                            writer?.WriteLine("{0}: {1}", name, line);
                    }
                }
            }
            catch
            {
                started?.Set();
            }
        }

        private void DoRun(string name, ProcessStartInfo info, ManualResetEvent started, string readyText)
        {
            try
            {
                _process = Process.Start(info);
                if (_process == null)
                    return;

                WindowsJobObject.Instance?.AddProcess(_process.Handle);

                try
                {
                    Process.GetCurrentProcess().ProcessorAffinity = new IntPtr((1 << Environment.ProcessorCount) - 1);
                    _process.ProcessorAffinity = new IntPtr((1 << Environment.ProcessorCount) - 1);
                }
                catch
                {
                    // ignored
                }

                if (string.IsNullOrWhiteSpace(readyText))
                    started?.Set();

                var outStreams = new TextWriter[2];
                var errStreams = new TextWriter[2];
#if DEBUG
                outStreams[0] = Debugger.IsAttached && !IsNullStream(Console.Out) ? Console.Out : null;
                errStreams[0] = Debugger.IsAttached && !IsNullStream(Console.Error) ? Console.Error : null;
#endif
                outStreams[1] = null;
                errStreams[1] = null;

                if (info.RedirectStandardOutput)
                {
                    var stdOut = _process.StandardOutput;
                    Task.Factory.StartNew(
                        () => ReadOutput(name, started, readyText, stdOut, outStreams),
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);
                }

                if (info.RedirectStandardError)
                {
                    var stdErr = _process.StandardError;
                    Task.Factory.StartNew(
                        () => ReadOutput(name, started, readyText, stdErr, errStreams),
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);
                }

                _process.WaitForExit();

                started?.Set();
            }
            finally
            {
                Dispose();
            }
        }

        private void Dispose(bool disposing)
        {
            if (_process == null)
                return;

            try
            {
                if (!_process.HasExited)
                    _process.Kill();
            }
            catch
            {
                // ignored
            }

            try
            {
                if (disposing)
                    _process.Dispose();
            }
            catch
            {
                // ignored
            }

            _process = null;
        }

        #endregion
    }
}