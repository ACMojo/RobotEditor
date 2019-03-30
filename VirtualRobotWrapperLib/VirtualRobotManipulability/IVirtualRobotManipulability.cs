using System;
using System.ServiceModel;

using ProtoBuf;
using ProtoBuf.ServiceModel;

using VirtualRobotWrapperLib.Wcf;

namespace VirtualRobotWrapperLib.VirtualRobotManipulability
{
    [ServiceContract, ProtoContract(IsGroup = true)]
    public interface IVirtualRobotManipulability : IRemoteService, IDisposable
    {
        #region Properties

        float[] MinBox
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        float[] MaxBox
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        float MaxManipulability
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        #endregion

        #region Public methods

        [OperationContract, ProtoBehavior]
        bool Init(int argc, string[] argv, string file, string robotNodeSetName, string baseName, string tcpName);

        [OperationContract, ProtoBehavior]
        ManipulabilityVoxel[] GetManipulabilityWithPenalty(
            float discrTr,
            float discrRot,
            int loops,
            bool fillHoles,
            bool manipulabilityAsMinMaxRatio,
            bool penalizeJointLimits,
            float jointLimitPenalizationFactor);

        [OperationContract, ProtoBehavior]
        ManipulabilityVoxel[] GetManipulability(float discrTr, float discrRot, int loops, bool fillHoles, bool manipulabilityAsMinMaxRatio);

        #endregion
    }
}