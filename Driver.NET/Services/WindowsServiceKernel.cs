namespace Driver.NET.Services
{
    using System;

    using Driver.NET.Windows;

    /// <summary>
    /// Loads and unloads a <see cref="WindowsService"/> as a kernel driver.
    /// </summary>
    public class WindowsServiceKernel
    {
        /// <summary>
        /// The name of the privilege required to load and unload kernel drivers.
        /// </summary>
        private const string LoadDriverPrivilege = "SeLoadDriverPrivilege";

        /// <summary>
        /// A native function taking the registry path of a service, such as NtLoadDriver or NtUnloadDriver.
        /// </summary>
        private delegate uint DriverOperation(ref WindowsService.UNICODE_STRING ServiceName);

        /// <summary>
        /// Gets the windows service this kernel service is based on.
        /// </summary>
        private WindowsService WindowsService
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsServiceKernel"/> class.
        /// </summary>
        /// <param name="InWindowsService">The windows service this kernel service is based on.</param>
        public WindowsServiceKernel(WindowsService InWindowsService)
        {
            this.WindowsService = InWindowsService ?? throw new ArgumentNullException(nameof(InWindowsService));
        }

        /// <summary>
        /// Attempts to start this kernel service on the local machine.
        /// </summary>
        /// <returns>The NTSTATUS returned by NtLoadDriver, zero on success.</returns>
        public ulong TryStartDriver()
        {
            return this.InvokeWithLoadDriverPrivilege(WindowsService.NtLoadDriver);
        }

        /// <summary>
        /// Attempts to stop this kernel service on the local machine.
        /// </summary>
        /// <returns>The NTSTATUS returned by NtUnloadDriver, zero on success.</returns>
        public ulong TryStopDriver()
        {
            return this.InvokeWithLoadDriverPrivilege(WindowsService.NtUnloadDriver);
        }

        /// <summary>
        /// Converts a Windows Service to a Windows Kernel Driver Service.
        /// </summary>
        /// <param name="Service">The service.</param>
        public static WindowsServiceKernel FromService(WindowsService Service)
        {
            return new WindowsServiceKernel(Service);
        }

        /// <summary>
        /// Calls the given native function with the native registry path of the service,
        /// while the privilege to load and unload kernel drivers is held.
        /// </summary>
        /// <param name="Operation">The native function to call.</param>
        private ulong InvokeWithLoadDriverPrivilege(DriverOperation Operation)
        {
            // 
            // Acquire the privilege to load and unload kernel drivers.
            // 

            WinApi.AddPrivilege(LoadDriverPrivilege);

            // 
            // Manually load or unload the driver by calling the Windows API.
            // 

            var ServiceName = new WindowsService.UNICODE_STRING(this.WindowsService.NativeRegistryPath);

            try
            {
                return Operation(ref ServiceName);
            }
            finally
            {
                // 
                // Release the native string and the privilege.
                // 

                ServiceName.Dispose();
                WinApi.RemovePrivilege(LoadDriverPrivilege);
            }
        }
    }
}
