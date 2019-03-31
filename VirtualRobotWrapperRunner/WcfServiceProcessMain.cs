using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using VirtualRobotWrapperLib.Wcf;

namespace VirtualRobotWrapperRunner
{
    public abstract class WcfServiceProcessMain
    {
        #region Enums

        [Flags]
        public enum ErrorModes : uint
        {
            // ReSharper disable InconsistentNaming

            SEM_NOGPFAULTERRORBOX = 0x0002,

            // ReSharper restore InconsistentNaming
        }

        #endregion

        #region Fields

        private static bool _attachDebugger;

        #endregion

        #region Public methods

        [DllImport("kernel32.dll")]
        public static extern ErrorModes SetErrorMode(ErrorModes uMode);

        public static void NotifyReady()
        {
            Console.Out.WriteLine(@"ready");
        }

        public static void NotifyDone()
        {
            Console.Out.WriteLine(@"done");
        }

        public static void ReportProgress(string progress)
        {
            Console.Error.WriteLine(progress);
        }

        public static void ReportError(Exception ex)
        {
            Console.Error.WriteLine($"Server error: {ex.Message} ({ex.GetType()})");
            Console.Error.WriteLine(ex);
        }

        public static bool IsDebugWcfEnabled(string[] args)
        {
            return args?.Any(a => a.ToUpperInvariant() == "DEBUGWCF") ?? false;
        }

        #endregion

        #region Protected methods

        protected static bool Setup(string[] args)
        {
            try
            {
                SetErrorMode(ErrorModes.SEM_NOGPFAULTERRORBOX);
            }
            catch
            {
                // ignored
            }

            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = new IntPtr((1 << Environment.ProcessorCount) - 1);
            }
            catch
            {
                // ignored
            }

            _attachDebugger = (args?.Any(a => a.ToUpperInvariant() == "DEBUG") ?? false) && !Debugger.IsAttached;

            try
            {
                if (_attachDebugger)
                    DebugHelper.AttachDebugger(Process.GetCurrentProcess().Id);
            }
            catch
            {
                _attachDebugger = false;
            }

            try
            {
                int workerThreads;
                int completionPortThreads;
                ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
                ThreadPool.SetMinThreads(workerThreads, completionPortThreads * 2);
            }
            catch
            {
                // ignored
            }

            return true;
        }

        protected static void Exit()
        {
            try
            {
                if (_attachDebugger)
                    DebugHelper.DetachDebugger(Process.GetCurrentProcess().Id);
            }
            catch
            {
                // ignored
            }

            ReportProgress("Process exit");
        }

        #endregion
    }
}