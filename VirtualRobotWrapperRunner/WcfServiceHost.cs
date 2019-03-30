using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;

using ProtoBuf.ServiceModel;

using VirtualRobotWrapperLib.Wcf;

namespace VirtualRobotWrapperRunner
{
    public static class WcfServiceHost
    {
        #region Public methods

        public static void Run(string name, IRemoteService obj, Type endPointType, string endPoint, bool debugWcf)
        {
            WcfServiceProcessMain.ReportProgress(@"Creating host...");

            using (var host = new ServiceHost(obj))
            {
                host.Description.Name = name;

                var debug = host.Description.Behaviors.Find<ServiceDebugBehavior>();

                if (debugWcf)
                {
                    if (debug == null)
                        host.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
                    else
                        debug.IncludeExceptionDetailInFaults = true;
                }
                else
                {
                    if (debug != null)
                        host.Description.Behaviors.Remove(debug);

                    var visualStudioDebug =
                        host.Description.Behaviors.FirstOrDefault(b => b.GetType().FullName == "Microsoft.VisualStudio.Diagnostics.ServiceModelSink.Behavior");
                    if (visualStudioDebug != null)
                        host.Description.Behaviors.Remove(visualStudioDebug);
                }

                host.Description.Behaviors.Add(new WcfErrorLogger());

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

                WcfServiceProcessMain.ReportProgress(@"Creating service endpoint...");

                var serviceEndpoint = host.AddServiceEndpoint(endPointType, binding, $"net.pipe://localhost/{endPoint}");
                serviceEndpoint.EndpointBehaviors.Add(new ProtoEndpointBehavior());

                WcfServiceProcessMain.ReportProgress(@"Opening host...");

                host.Open();

                WcfServiceProcessMain.ReportProgress(@"Host opened");

                WcfServiceProcessMain.NotifyReady();

                while (!obj.ExitRequested)
                {
                    try
                    {
                        Process.GetCurrentProcess().ProcessorAffinity = new IntPtr((1 << Environment.ProcessorCount) - 1);
                    }
                    catch
                    {
                        // ignored
                    }

                    Thread.Sleep(50);
                }

                WcfServiceProcessMain.ReportProgress(@"Exiting...");

                WcfServiceProcessMain.NotifyDone();

                host.Close();

                WcfServiceProcessMain.ReportProgress(@"Host closed");
            }
        }

        #endregion

        #region Nested types

        private class WcfErrorLogger : IErrorHandler, IServiceBehavior
        {
            #region Public methods

            public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
            {
                // NOP
            }

            public bool HandleError(Exception error)
            {
                if (error == null || error.GetType() == typeof(CommunicationException))
                    return true;

                WcfServiceProcessMain.ReportError(error);

                return true;
            }

            public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                // NOP
            }

            public void AddBindingParameters(
                ServiceDescription serviceDescription,
                ServiceHostBase serviceHostBase,
                Collection<ServiceEndpoint> endpoints,
                BindingParameterCollection bindingParameters)
            {
                // NOP
            }

            public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
                    channelDispatcher.ErrorHandlers.Add(this);
            }

            #endregion
        }

        #endregion
    }
}