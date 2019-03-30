using System;
using System.Linq;

namespace VirtualRobotWrapperRunner
{
    public class VirtualRobotWrapperRunnerMain : WcfServiceProcessMain
    {
        #region Public methods

        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static void Main(string[] args)
        {
            if (args == null)
                return;

            if (!Setup(args))
                return;

            try
            {
                var service = new VirtualRobotWrapperRunnerService();

                if (args.Any(a => string.Equals("OBB", a)))
                    service.RunObbWrapper(args);
                else
                    service.RunVirtualRobotManipulability(args);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }

            Exit();
        }

        #endregion
    }
}