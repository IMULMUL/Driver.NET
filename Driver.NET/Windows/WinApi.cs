namespace Driver.NET.Windows
{
    using System;
    using System.Runtime.InteropServices;

    internal static class WinApi
    {
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueW", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LookupPrivilegeValue(string SystemName, string Name, out long Luid);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr Handle);

        /// <summary>
        /// A TOKEN_PRIVILEGES structure holding a single LUID_AND_ATTRIBUTES entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public long Luid;
            public int Attributes;
        }

        private const int SE_PRIVILEGE_DISABLED = 0x00000000;
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const int ERROR_SUCCESS = 0;

        /// <summary>
        /// Enables the given privilege on the token of the current process.
        /// </summary>
        /// <param name="Privilege">The name of the privilege, e.g. 'SeLoadDriverPrivilege'.</param>
        /// <returns><c>true</c> if the privilege is now enabled; otherwise, <c>false</c>.</returns>
        internal static bool AddPrivilege(string Privilege)
        {
            return AdjustPrivilege(Privilege, SE_PRIVILEGE_ENABLED);
        }

        /// <summary>
        /// Disables the given privilege on the token of the current process.
        /// </summary>
        /// <param name="Privilege">The name of the privilege, e.g. 'SeLoadDriverPrivilege'.</param>
        /// <returns><c>true</c> if the privilege is now disabled; otherwise, <c>false</c>.</returns>
        internal static bool RemovePrivilege(string Privilege)
        {
            return AdjustPrivilege(Privilege, SE_PRIVILEGE_DISABLED);
        }

        /// <summary>
        /// Sets the attributes of the given privilege on the token of the current process.
        /// </summary>
        /// <param name="Privilege">The name of the privilege.</param>
        /// <param name="Attributes">The attributes to set, e.g. SE_PRIVILEGE_ENABLED.</param>
        private static bool AdjustPrivilege(string Privilege, int Attributes)
        {
            var TokenHandle = IntPtr.Zero;

            try
            {
                if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out TokenHandle))
                {
                    TokenHandle = IntPtr.Zero;
                    return false;
                }

                var Privileges = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Attributes = Attributes
                };

                if (!LookupPrivilegeValue(null, Privilege, out Privileges.Luid))
                {
                    return false;
                }

                if (!AdjustTokenPrivileges(TokenHandle, false, ref Privileges, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    return false;
                }

                // 
                // AdjustTokenPrivileges succeeds even when the privilege is not held by the token,
                // in which case the last error is set to ERROR_NOT_ALL_ASSIGNED.
                // 

                return Marshal.GetLastWin32Error() == ERROR_SUCCESS;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (TokenHandle != IntPtr.Zero)
                {
                    CloseHandle(TokenHandle);
                }
            }
        }
    }
}
