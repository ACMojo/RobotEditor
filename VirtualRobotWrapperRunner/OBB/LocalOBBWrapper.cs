using System.ServiceModel;

using MathGeoLibWrapper;

using VirtualRobotWrapperLib.OBB;

namespace VirtualRobotWrapperRunner.OBB
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class LocalOBBWrapper : IOBBWrapper
    {
        #region Fields

        private OBBWrapper _instance;

        #endregion

        #region Properties

        public bool ExitRequested { get; private set; }
        public bool IsRemote => false;

        public double[] Position => _instance.Position;
        public double[] HalfExtents => _instance.HalfExtents;
        public double[][] Axis => _instance.Axis;

        #endregion

        #region Public methods

        public void Calculate(double[][] points)
        {
            //try
            //{
            //    _instance?.Dispose();
            //}
            //catch
            //{
            //    // ignored
            //}

            _instance = new OBBWrapper(points);
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
            //try
            //{
            //    _instance?.Dispose();
            //}
            //catch
            //{
            //    // ignored
            //}

            _instance = null;
        }

        #endregion
    }
}