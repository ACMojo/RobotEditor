using System.ServiceModel;

using ProtoBuf;
using ProtoBuf.ServiceModel;

namespace VirtualRobotWrapperLib.Wcf
{
    [ServiceContract, ProtoContract(IsGroup = true)]
    public interface IRemoteService
    {
        #region Properties

        bool ExitRequested
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        bool IsRemote
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        #endregion

        #region Public methods

        [OperationContract, ProtoBehavior]
        bool Running();

        [OperationContract(IsOneWay = false), ProtoBehavior]
        void Exit();

        #endregion
    }
}