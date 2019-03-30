#if DEBUG
//#define DEBUG_WCF
//#define DEBUG_SERVER
#endif

namespace VirtualRobotWrapperLib.Wcf
{
    public abstract class WcfServiceProcessRunnerBase
    {
        #region Fields

#if DEBUG_WCF
        protected static readonly bool DebugWcf = true;
#else
        protected static readonly bool DebugWcf = false;
#endif

#if DEBUG_SERVER
        protected static readonly bool DebugServer = true;
#else
        protected static readonly bool DebugServer = false;
#endif

        #endregion
    }
}