namespace Driver.NET.Services
{
    using System;
    using System.Runtime.InteropServices;

    public partial class WindowsService
    {
        /// <summary>
        /// The native UNICODE_STRING structure, owning an unmanaged copy of the managed string it was built from.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct UNICODE_STRING : IDisposable
        {
            internal ushort Length;
            internal ushort MaximumLength;
            private IntPtr Buffer;

            internal UNICODE_STRING(string s)
            {
                this.Length = (ushort) (s.Length * 2);
                this.MaximumLength = (ushort) (this.Length + 2);
                this.Buffer = Marshal.StringToHGlobalUni(s);
            }

            public void Dispose()
            {
                if (this.Buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.Buffer);
                    this.Buffer = IntPtr.Zero;
                }

                this.Length = 0;
                this.MaximumLength = 0;
            }

            public override string ToString()
            {
                return this.Buffer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(this.Buffer);
            }
        }

        [DllImport("ntdll.dll", EntryPoint = "NtLoadDriver", ExactSpelling = true)]
        internal static extern uint NtLoadDriver(
            ref UNICODE_STRING ServiceName
        );

        [DllImport("ntdll.dll", EntryPoint = "NtUnloadDriver", ExactSpelling = true)]
        internal static extern uint NtUnloadDriver(
            ref UNICODE_STRING ServiceName
        );
    }
}
