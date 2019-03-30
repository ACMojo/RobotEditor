using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace VirtualRobotWrapperLib.Wcf
{
    public class WindowsJobObject : IDisposable
    {
        #region Enums

        private enum JobObjectInfoType
        {
            ExtendedLimitInformation = 9
        }

        #endregion

        #region Fields

        private IntPtr _handle;
        private bool _disposed;

        #endregion

        #region Instance

        public WindowsJobObject()
        {
            _handle = CreateJobObject(IntPtr.Zero, null);

            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = 0x2000
            };

            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = info
            };

            var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            var extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!SetInformationJobObject(_handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                throw new Exception($@"Unable to set information.  Error: {Marshal.GetLastWin32Error()}");
        }

        #endregion

        #region Properties

        public static WindowsJobObject Instance { get; set; }

        #endregion

        #region Public methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }

        public bool AddProcess(IntPtr processHandle)
        {
            return AssignProcessToJobObject(_handle, processHandle);
        }

        #endregion

        #region Private methods

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr a, string lpName);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr job);

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing) { }

            Close();
            _disposed = true;
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct IO_COUNTERS
        {
            #region Fields and properties

            public readonly ulong ReadOperationCount;
            public readonly ulong WriteOperationCount;
            public readonly ulong OtherOperationCount;
            public readonly ulong ReadTransferCount;
            public readonly ulong WriteTransferCount;
            public readonly ulong OtherTransferCount;

            #endregion
        }

        [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            #region Fields and properties

            public readonly long PerProcessUserTimeLimit;
            public readonly long PerJobUserTimeLimit;
            public uint LimitFlags;
            public readonly UIntPtr MinimumWorkingSetSize;
            public readonly UIntPtr MaximumWorkingSetSize;
            public readonly uint ActiveProcessLimit;
            public readonly UIntPtr Affinity;
            public readonly uint PriorityClass;
            public readonly uint SchedulingClass;

            #endregion
        }

        [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            #region Fields and properties

            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public readonly IO_COUNTERS IoInfo;
            public readonly UIntPtr ProcessMemoryLimit;
            public readonly UIntPtr JobMemoryLimit;
            public readonly UIntPtr PeakProcessMemoryUsed;
            public readonly UIntPtr PeakJobMemoryUsed;

            #endregion
        }

        #endregion
    }
}