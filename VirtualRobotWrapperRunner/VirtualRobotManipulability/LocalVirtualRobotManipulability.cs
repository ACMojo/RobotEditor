using System.Linq;
using System.ServiceModel;

using VirtualRobotWrapperLib.VirtualRobotManipulability;

namespace VirtualRobotWrapperRunner.VirtualRobotManipulability
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class LocalVirtualRobotManipulability : IVirtualRobotManipulability
    {
        #region Fields

        private readonly VirtualRobotWrapper.VirtualRobotManipulability _instance = new VirtualRobotWrapper.VirtualRobotManipulability();

        #endregion

        #region Instance

        ~LocalVirtualRobotManipulability()
        {
            Dispose();
        }

        #endregion

        #region Properties

        public bool ExitRequested { get; private set; }
        public bool IsRemote => false;

        public float[] MinBox => _instance.MinBox;
        public float[] MaxBox => _instance.MinBox;
        public float MaxManipulability => _instance.MaxManipulability;

        #endregion

        #region Public methods

        public bool Init(int argc, string[] argv, string file, string robotNodeSetName, string baseName, string tcpName)
        {
            return _instance.Init(argc, argv, file, robotNodeSetName, baseName, tcpName);
        }

        public ManipulabilityVoxel[] GetManipulability(
            float discrTr,
            float discrRot,
            int loops,
            bool fillHoles,
            bool manipulabilityAsMinMaxRatio,
            bool penalizeJointLimits,
            float jointLimitPenalizationFactor)
        {
            var result = _instance.GetManipulability(
                discrTr,
                discrRot,
                loops,
                fillHoles,
                manipulabilityAsMinMaxRatio,
                penalizeJointLimits,
                jointLimitPenalizationFactor);

            return result.Select(r => new ManipulabilityVoxel { A = r.a, B = r.b, C = r.c, X = r.x, Y = r.y, Z = r.z, Value = r.value }).ToArray();
        }

        public ManipulabilityVoxel[] GetManipulability(float discrTr, float discrRot, int loops, bool fillHoles, bool manipulabilityAsMinMaxRatio)
        {
            var result = _instance.GetManipulability(discrTr, discrRot, loops, fillHoles, manipulabilityAsMinMaxRatio);

            return result.Select(r => new ManipulabilityVoxel { A = r.a, B = r.b, C = r.c, X = r.x, Y = r.y, Z = r.z, Value = r.value }).ToArray();
        }

        public bool Running()
        {
            return true;
        }

        public void Exit()
        {
            ExitRequested = true;
        }

        public void Dispose()
        {
            _instance.Dispose();
        }

        #endregion
    }
}