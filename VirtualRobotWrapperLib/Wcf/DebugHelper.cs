using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace VirtualRobotWrapperLib.Wcf
{
    public class DebugHelper
    {
        #region Fields

        private static readonly List<uint> _retryCodes =
            new List<uint>(new uint[] { 0x00030202, 0x80010109, 0x8001010A, 0x8DEAD01E });

        private static readonly int MAX_RETRIES = 500;
        private static int _visualStudioVersion = -1;
        private static PropertyInfo _debuggerProperty;
        private static PropertyInfo _localProcessesProperty;
        private static MethodInfo _enumeratorMethod;
        private static PropertyInfo _processIdProperty;
        private static MethodInfo _attachMethod;
        private static MethodInfo _detachMethod;

        #endregion

        #region Public methods

        [STAThread]
        public static void AttachDebugger(int processId)
        {
            try
            {
                if (!Setup())
                    return;

                var dte = Marshal.GetActiveObject($@"VisualStudio.DTE.{_visualStudioVersion}.0");

                var debugger = _debuggerProperty.GetValue(dte, null);
                var localProcesses = _localProcessesProperty.GetValue(debugger, null);

                var retryCount = 0;
                while (retryCount < MAX_RETRIES)
                {
                    retryCount++;

                    try
                    {
                        try
                        {
                            MessageFilter.Register();

                            var enumerator = (IEnumerator)_enumeratorMethod.Invoke(localProcesses, null);

                            object process = null;
                            while (process == null && enumerator.MoveNext())
                            {
                                if ((int)_processIdProperty.GetValue(enumerator.Current, null) != processId)
                                    continue;

                                process = enumerator.Current;
                            }

                            _attachMethod.Invoke(process, null);
                            break;
                        }
                        catch (TargetInvocationException ex)
                        {
                            if (ex.InnerException != null)
                                throw ex.InnerException;

                            throw;
                        }
                        finally
                        {
                            MessageFilter.Revoke();
                        }
                    }
                    catch (COMException ex)
                    {
                        if (_retryCodes.Contains((uint)ex.ErrorCode))
                            continue;

                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        [STAThread]
        public static void DetachDebugger(int processId)
        {
            try
            {
                if (!Setup())
                    return;

                var dte = Marshal.GetActiveObject($@"VisualStudio.DTE.{_visualStudioVersion}.0");

                var debugger = _debuggerProperty.GetValue(dte, null);
                var localProcesses = _localProcessesProperty.GetValue(debugger, null);

                var retryCount = 0;
                while (retryCount < MAX_RETRIES)
                {
                    retryCount++;

                    try
                    {
                        try
                        {
                            MessageFilter.Register();

                            var enumerator = (IEnumerator)_enumeratorMethod.Invoke(localProcesses, null);

                            object process = null;
                            while (process == null && enumerator.MoveNext())
                            {
                                if ((int)_processIdProperty.GetValue(enumerator.Current, null) != processId)
                                    continue;

                                process = enumerator.Current;
                            }

                            _detachMethod.Invoke(process, new object[] { false });

                            break;
                        }
                        catch (TargetInvocationException ex)
                        {
                            if (ex.InnerException != null)
                                throw ex.InnerException;

                            throw;
                        }
                        finally
                        {
                            MessageFilter.Revoke();
                        }
                    }
                    catch (COMException ex)
                    {
                        if (_retryCodes.Contains((uint)ex.ErrorCode))
                            continue;

                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Private methods

        private static bool Setup()
        {
            if (_debuggerProperty != null
                && _localProcessesProperty != null
                && _enumeratorMethod != null
                && _processIdProperty != null
                && _attachMethod != null
                && _detachMethod != null
                && _visualStudioVersion != -1)
                return true;

            var visualStudioPath = GetVisualStudioInstallDirectory();
            if (visualStudioPath == null)
                return false;

            if (_visualStudioVersion == -1)
                return false;

            var envDtePath = Path.Combine(visualStudioPath, @"PublicAssemblies\EnvDTE.dll");
            if (!File.Exists(envDtePath))
            {
                var files = Directory.GetFiles(Path.Combine(visualStudioPath, @"Extensions"), "EnvDTE.dll", SearchOption.AllDirectories);
                envDtePath = files.FirstOrDefault();
                if (!File.Exists(envDtePath))
                    return false;
            }

            if (string.IsNullOrWhiteSpace(envDtePath))
                return false;

            var assembly = Assembly.LoadFrom(envDtePath);

            var dteType = assembly.GetType("EnvDTE._DTE", false, true);
            var debuggerType = assembly.GetType("EnvDTE.Debugger", false, true);
            var processesType = assembly.GetType("EnvDTE.Processes", false, true);
            var processType = assembly.GetType("EnvDTE.Process", false, true);

            _debuggerProperty = dteType.GetProperty("Debugger", debuggerType);
            _localProcessesProperty = debuggerType.GetProperty("LocalProcesses", processesType);
            _enumeratorMethod = processesType.GetMethod("GetEnumerator", new Type[] { });
            _processIdProperty = processType.GetProperty("ProcessID", typeof(int));
            _attachMethod = processType.GetMethod("Attach", new Type[] { });
            _detachMethod = processType.GetMethod("Detach", new[] { typeof(bool) });

            return true;
        }

        private static string GetVisualStudioInstallDirectory()
        {
            try
            {
                _visualStudioVersion = -1;

                var softwareKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (softwareKey == null)
                    return null;

                if (IntPtr.Size == 8)
                {
                    var wow6432Key = softwareKey.OpenSubKey("Wow6432Node");
                    if (wow6432Key == null)
                        return null;

                    softwareKey.Close();
                    softwareKey = wow6432Key;
                }

                var microsoftKey = softwareKey.OpenSubKey("Microsoft");
                if (microsoftKey == null)
                {
                    softwareKey.Close();
                    return null;
                }

                var vsKey = microsoftKey.OpenSubKey("VisualStudio");
                if (vsKey == null)
                {
                    microsoftKey.Close();
                    softwareKey.Close();
                    return null;
                }

                var vsVersionKey = vsKey.OpenSubKey(@"15.0\\Setup");
                if (vsVersionKey != null)
                {
                    _visualStudioVersion = 15;
                }

                for (var i = 14; i >= 7 && vsVersionKey == null; i--)
                {
                    vsVersionKey = vsKey.OpenSubKey($@"{i}.0\\Setup\\VS");
                    _visualStudioVersion = i;
                }

                if (vsVersionKey == null)
                {
                    _visualStudioVersion = -1;
                    vsKey.Close();
                    microsoftKey.Close();
                    softwareKey.Close();
                    return null;
                }

                var ret = _visualStudioVersion == 15
                              ? Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\Common7\IDE")
                              : vsVersionKey.GetValue("EnvironmentDirectory");

                vsVersionKey.Close();
                vsKey.Close();
                microsoftKey.Close();
                softwareKey.Close();

                return ret?.ToString();
            }
            catch
            {
                // ignored
            }

            return null;
        }

        #endregion

        #region Nested types

        private class MessageFilter : IOleMessageFilter
        {
            #region Public methods

            public static void Register()
            {
                IOleMessageFilter newFilter = new MessageFilter();
                IOleMessageFilter unused;
                CoRegisterMessageFilter(newFilter, out unused);
            }

            public static void Revoke()
            {
                IOleMessageFilter unused;
                CoRegisterMessageFilter(null, out unused);
            }

            #endregion

            #region Private methods

            [DllImport("Ole32.dll")]
            private static extern int CoRegisterMessageFilter(
                IOleMessageFilter newFilter,
                out
                    IOleMessageFilter
                    oldFilter);

            int IOleMessageFilter.HandleInComingCall(
                int dwCallType,
                IntPtr hTaskCaller,
                int dwTickCount,
                IntPtr
                    lpInterfaceInfo)
            {
                //Return the flag SERVERCALL_ISHANDLED.
                return 0;
            }

            // Thread call was rejected, so try again.
            int IOleMessageFilter.RetryRejectedCall(
                IntPtr
                    hTaskCallee,
                int dwTickCount,
                int dwRejectType)
            {
                if (dwRejectType == 2)
                    // flag = SERVERCALL_RETRYLATER.
                {
                    // Retry the thread call immediately if return >=0 & 
                    // <100.
                    return 99;
                }

                // Too busy; cancel call.
                return -1;
            }

            int IOleMessageFilter.MessagePending(
                IntPtr hTaskCallee,
                int dwTickCount,
                int dwPendingType)
            {
                //Return the flag PENDINGMSG_WAITDEFPROCESS.
                return 2;
            }

            #endregion
        }

        [ComImport, Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleMessageFilter
        {
            #region Public methods

            [PreserveSig]
            int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

            [PreserveSig]
            int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);

            [PreserveSig]
            int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);

            #endregion
        }

        #endregion
    }
}