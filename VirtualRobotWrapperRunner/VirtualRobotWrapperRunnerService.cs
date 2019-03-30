using VirtualRobotWrapperLib.OBB;
using VirtualRobotWrapperLib.VirtualRobotManipulability;

using VirtualRobotWrapperRunner.OBB;
using VirtualRobotWrapperRunner.VirtualRobotManipulability;

namespace VirtualRobotWrapperRunner
{
    internal class VirtualRobotWrapperRunnerService
    {
        #region Public methods

        public void RunObbWrapper(string[] args)
        {
            if (args == null)
                return;

            var endPoint = args.Length > 0 && !string.IsNullOrEmpty(args[0]) ? args[0] : @"OBBWrapper";
            var debugWcf = WcfServiceProcessMain.IsDebugWcfEnabled(args);

            WcfServiceHost.Run("OBBWrapperService", new LocalObbWrapper(), typeof(IObbWrapper), endPoint, debugWcf);
        }

        public void RunVirtualRobotManipulability(string[] args)
        {
            if (args == null)
                return;

            var endPoint = args.Length > 0 && !string.IsNullOrEmpty(args[0]) ? args[0] : @"VirtualRobotManipulability";
            var debugWcf = WcfServiceProcessMain.IsDebugWcfEnabled(args);

            WcfServiceHost.Run(
                "VirtualRobotManipulabilityService",
                new LocalVirtualRobotManipulability(),
                typeof(IVirtualRobotManipulability),
                endPoint,
                debugWcf);
        }

        #endregion
    }
}