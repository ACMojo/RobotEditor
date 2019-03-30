using System;
using System.ServiceModel;

using VirtualRobotWrapperLib.Wcf;

namespace VirtualRobotWrapperLib.OBB
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class RemoteOBBWrapper : WcfServiceProcessRunner<IOBBWrapper>, IOBBWrapper
    {
        #region Properties

        protected override string ProcessName => "VirtualRobotWrapperRunner.exe";

        protected override string ServerName => "RemoteOBBWrapper";

        protected override string ProcessArguments => "OBB";

        #endregion

        public double[] Position
        {
            get
            {
                try
                {
                    WaitForStart();

                    return Pipe.Position;
                }
                catch (Exception)
                {
                    if (Channel.State != CommunicationState.Faulted)
                        throw;

                    Restart();

                    return Pipe.Position;
                }
            }
        }

        public double[] HalfExtents
        {
            get
            {
                try
                {
                    WaitForStart();

                    return Pipe.HalfExtents;
                }
                catch (Exception)
                {
                    if (Channel.State != CommunicationState.Faulted)
                        throw;

                    Restart();

                    return Pipe.HalfExtents;
                }
            }
        }

        public double[][] Axis
        {
            get
            {
                try
                {
                    WaitForStart();

                    return Pipe.Axis;
                }
                catch (Exception)
                {
                    if (Channel.State != CommunicationState.Faulted)
                        throw;

                    Restart();

                    return Pipe.Axis;
                }
            }
        }

        public void Calculate(double[][] points)
        {
            try
            {
                WaitForStart();

                Pipe.Calculate(points);
            }
            catch (Exception)
            {
                if (Channel.State != CommunicationState.Faulted)
                    throw;

                Restart();

                Pipe.Calculate(points);
            }
        }
    }
}