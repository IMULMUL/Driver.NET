namespace Driver.NET.DeviceIoControl
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    public partial class DeviceIoControl
    {
        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", ExactSpelling = true, SetLastError = true)]
        internal static extern unsafe bool NtDeviceIoControl(
            SafeFileHandle Handle,
            uint IoControlCode,
            void* InputBuffer,
            int InputBufferSize,
            void* OutputBuffer,
            int OutputBufferSize,
            out int ReturnedBytes,
            IntPtr Overlapped
        );

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            string FileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess FileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare FileShare,
            IntPtr SecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode CreationDisposition,
            uint FlagsAndAttributes,
            IntPtr Template
        );
    }
}
