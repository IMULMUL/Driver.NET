namespace Driver.NET.DeviceIoControl
{
    using System;
    using System.IO;

    public partial class DeviceIoControl
    {
        /// <summary>
        /// Checks if the specified symbolic file exists and can be opened.
        /// </summary>
        /// <param name="SymbolicName">The path of the symbolic file.</param>
        public static bool Exists(string SymbolicName)
        {
            if (string.IsNullOrEmpty(SymbolicName))
            {
                return false;
            }

            using (var Handle = CreateFile(SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
            {
                return !Handle.IsInvalid;
            }
        }
    }
}
