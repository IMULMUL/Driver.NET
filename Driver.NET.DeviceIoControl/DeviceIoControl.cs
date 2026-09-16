namespace Driver.NET.DeviceIoControl
{
    using System;
    using System.IO;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Opens a handle to a device through its symbolic link and sends I/O control requests to the driver behind it.
    /// </summary>
    public partial class DeviceIoControl : IDeviceIo, IDisposable
    {
        /// <summary>
        /// Gets the safe file handle.
        /// </summary>
        public SafeFileHandle Handle
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path to the symbolic link.
        /// </summary>
        public string SymbolicName
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the handle is valid.
        /// </summary>
        public bool IsValid => this.Handle != null && !this.Handle.IsInvalid && !this.Handle.IsClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceIoControl"/> class.
        /// </summary>
        /// <param name="InSymbolicName">The name of the symbolic link, e.g. '\\.\MyDevice'.</param>
        public DeviceIoControl(string InSymbolicName)
        {
            if (string.IsNullOrEmpty(InSymbolicName))
            {
                throw new ArgumentNullException(nameof(InSymbolicName), "The symbolic name is null or empty.");
            }

            this.SymbolicName = InSymbolicName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceIoControl"/> class.
        /// </summary>
        /// <param name="InHandle">A handle to a symbolic link.</param>
        public DeviceIoControl(SafeFileHandle InHandle)
        {
            this.Handle = InHandle;

            if (!this.IsValid)
            {
                throw new ArgumentException("The handle is invalid.", nameof(InHandle));
            }
        }

        /// <summary>
        /// Opens a handle to the symbolic link file. Check <see cref="IsValid"/> afterwards to know whether it succeeded.
        /// </summary>
        public void Connect()
        {
            if (this.SymbolicName == null)
            {
                throw new InvalidOperationException("This instance was created from an existing handle and has no symbolic name to connect to.");
            }

            // 
            // Close any handle previously opened, then open a handle to the symbolic file.
            // 

            this.Close();
            this.Handle = CreateFile(this.SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        }

        /// <summary>
        /// Closes the file handle previously opened to the symbolic link.
        /// </summary>
        public void Close()
        {
            this.Handle?.Dispose();
            this.Handle = null;
        }

        /// <summary>
        /// Closes the file handle previously opened to the symbolic link.
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }
    }
}
