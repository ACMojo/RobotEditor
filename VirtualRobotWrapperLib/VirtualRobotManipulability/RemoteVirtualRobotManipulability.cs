using System;
using System.ServiceModel;

using VirtualRobotWrapperLib.Wcf;

namespace VirtualRobotWrapperLib.VirtualRobotManipulability
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class RemoteVirtualRobotManipulability : WcfServiceProcessRunner<IVirtualRobotManipulability>, IVirtualRobotManipulability
    {
        #region Properties

        public float[] MinBox
        {
            get
            {
                try
                {
                    WaitForStart();

                    return Pipe.MinBox;
                }
                catch (Exception)
                {
                    if (Channel.State != CommunicationState.Faulted)
                        throw;

                    Restart();

                    return Pipe.MinBox;
                }
            }
        }

        public float[] MaxBox
        {
            get
            {
                try
                {
                    WaitForStart();

                    return Pipe.MaxBox;
                }
                catch (Exception)
                {
                    if (Channel.State != CommunicationState.Faulted)
                        throw;

                    Restart();

                    return Pipe.MaxBox;
                }
            }
        }

        public float MaxManipulability
        {
            get
            {
                try
                {
                    WaitForStart();

                    return Pipe.MaxManipulability;
                }
                catch (Exception)
                {
                    if (Channel.State != CommunicationState.Faulted)
                        throw;

                    Restart();

                    return Pipe.MaxManipulability;
                }
            }
        }

        protected override string ProcessName => "VirtualRobotWrapperRunner.exe";

        protected override string ServerName => "RemoteVirtualRobotManipulability";

        protected override string ProcessArguments => "Manipulability";

        #endregion

        #region Public methods

        public ManipulabilityVoxel[] GetManipulability(
            float discrTr,
            float discrRot,
            int loops,
            bool fillHoles,
            bool manipulabilityAsMinMaxRatio,
            bool penalizeJointLimits,
            float jointLimitPenalizationFactor)
        {
            try
            {
                WaitForStart();

                return Pipe.GetManipulability(
                    discrTr,
                    discrRot,
                    loops,
                    fillHoles,
                    manipulabilityAsMinMaxRatio,
                    penalizeJointLimits,
                    jointLimitPenalizationFactor);
            }
            catch (Exception)
            {
                if (Channel.State != CommunicationState.Faulted)
                    throw;

                Restart();

                return Pipe.GetManipulability(
                    discrTr,
                    discrRot,
                    loops,
                    fillHoles,
                    manipulabilityAsMinMaxRatio,
                    penalizeJointLimits,
                    jointLimitPenalizationFactor);
            }
        }

        public ManipulabilityVoxel[] GetManipulability(float discrTr, float discrRot, int loops, bool fillHoles, bool manipulabilityAsMinMaxRatio)
        {
            try
            {
                WaitForStart();

                return Pipe.GetManipulability(discrTr, discrRot, loops, fillHoles, manipulabilityAsMinMaxRatio);
            }
            catch (Exception)
            {
                if (Channel.State != CommunicationState.Faulted)
                    throw;

                Restart();

                return Pipe.GetManipulability(discrTr, discrRot, loops, fillHoles, manipulabilityAsMinMaxRatio);
            }
        }

        public bool Init(int argc, string[] argv, string file, string robotNodeSetName, string baseName, string tcpName)
        {
            try
            {
                WaitForStart();

                return Pipe.Init(argc, argv, file, robotNodeSetName, baseName, tcpName);
            }
            catch (Exception)
            {
                if (Channel.State != CommunicationState.Faulted)
                    throw;

                Restart();

                return Pipe.Init(argc, argv, file, robotNodeSetName, baseName, tcpName);
            }
        }

        #endregion
    }
}