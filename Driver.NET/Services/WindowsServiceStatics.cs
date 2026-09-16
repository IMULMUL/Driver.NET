namespace Driver.NET.Services
{
    using System;
    using System.Linq;
    using System.Security.AccessControl;

    using Microsoft.Win32;

    public partial class WindowsService
    {
        /// <summary>
        /// The relative (to local machine) registry path for the 'Services' key, where Windows Services configurations are stored.
        /// </summary>
        private const string RegistryPathForServicesRelative = @"SYSTEM\CurrentControlSet\Services";

        /// <summary>
        /// The full registry path for the 'Services' key, where Windows Services configurations are stored.
        /// </summary>
        private const string RegistryPathForServices = LocalMachineHive + "\\" + RegistryPathForServicesRelative;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsService"/> class using a service name.
        /// </summary>
        /// <param name="ServiceName">The name of the service.</param>
        public static WindowsService FromServiceName(string ServiceName)
        {
            if (string.IsNullOrEmpty(ServiceName))
            {
                throw new ArgumentNullException(nameof(ServiceName), "The service name is null or empty.");
            }

            return WindowsService.FromRegistryPath(RegistryPathForServices + "\\" + ServiceName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsService"/> class using a registry path.
        /// </summary>
        /// <param name="RegistryPath">The registry path of the service, starting with 'HKEY_LOCAL_MACHINE'.</param>
        public static WindowsService FromRegistryPath(string RegistryPath)
        {
            if (string.IsNullOrEmpty(RegistryPath))
            {
                throw new ArgumentNullException(nameof(RegistryPath), "The registry path is null or empty.");
            }

            //
            // Convert all forward slashes '/' to backward slashes '\' and remove any trailing slash.
            //

            RegistryPath = RegistryPath.Replace('/', '\\').TrimEnd('\\');

            //
            // Check the format of the registry path.
            //   - Format: HKEY_LOCAL_MACHINE\...
            //

            if (!RegistryPath.StartsWith(LocalMachineHive + "\\", StringComparison.Ordinal))
            {
                throw new ArgumentException("The registry path does not start with the local machine hive base path.", nameof(RegistryPath));
            }

            return new WindowsService
            {
                RegistryPath = RegistryPath
            };
        }

        /// <summary>
        /// Determines whether the given service is installed (present in registry) on the local machine.
        /// </summary>
        /// <param name="ServiceName">The name of the service.</param>
        /// <returns>
        ///   <c>true</c> if the specified service is installed; otherwise, <c>false</c>.
        /// </returns>
        public static bool Exists(string ServiceName)
        {
            if (string.IsNullOrEmpty(ServiceName))
            {
                return false;
            }

            //
            // Open the 'Services' key in registry and check if the service we are searching for is one of its sub-keys.
            //

            using (var ServicesKey = Registry.LocalMachine.OpenSubKey(RegistryPathForServicesRelative, RegistryRights.ReadKey))
            {
                if (ServicesKey == null)
                {
                    throw new InvalidOperationException("Failed to open the services registry key.");
                }

                return ServicesKey.GetSubKeyNames().Any(T => string.Equals(T, ServiceName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
