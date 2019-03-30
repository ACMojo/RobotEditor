using System;
using System.ServiceModel;

using ProtoBuf;
using ProtoBuf.ServiceModel;

using VirtualRobotWrapperLib.Wcf;

namespace VirtualRobotWrapperLib.OBB
{
    [ServiceContract, ProtoContract(IsGroup = true)]
    public interface IOBBWrapper : IRemoteService, IDisposable
    {
        #region Properties

        double[] Position
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        double[] HalfExtents
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        double[][] Axis
        {
            [OperationContract, ProtoBehavior]
            get;
        }

        #endregion

        #region Public methods

        [OperationContract, ProtoBehavior]
        void Calculate(double[][] points);

        #endregion
    }
}