namespace Driver.NET.Example
{
    using System;
    using System.Runtime.InteropServices;

    using Driver.NET.DeviceIoControl;
    using Driver.NET.Example.Enums;
    using Driver.NET.Example.Structures;
    using Driver.NET.Services;

    using Microsoft.Win32;

    internal static class Program
    {
        /// <summary>
        /// The IOCTL for storage query property.
        /// </summary>
        private const uint IOCTL_STORAGE_QUERY_PROPERTY = 0x002D1400;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static void Main()
        {
            // Example_CreateService();
            Example_QueryDiskSerial();
            Console.ReadKey();
        }

        /// <summary>
        /// Creates and configures a service, loads it as a kernel driver, then removes it.
        /// </summary>
        public static void Example_CreateService()
        {
            //
            // Ensure the registry key exists.
            //

            var Service = WindowsService.FromServiceName("mydriver");
            Service.CreateRegistryKey();

            //
            // Write values to the service's registry key.
            //

            Service.WriteRegistryValue("ImagePath", "System32\\drivers\\mydriver.sys");
            Service.WriteRegistryValue("Type", 1);
            Service.WriteRegistryValue("ErrorControl", 1);
            Service.WriteRegistryValue("Start", 1);
            Service.WriteRegistryValue("Group", "System Reserved");
            Service.WriteRegistryValue("DisplayName", "@mydriver.inf,%mydriver_ServiceDesc%;Custom Example Driver", RegistryValueKind.ExpandString);
            Service.WriteRegistryValue("Owners", new[] { "mydriver.inf" });
            Service.WriteRegistryValue("DependOnService", new[] { "myotherdriver" });

            //
            // Writes values to a sub-key of the service's registry key.
            //

            Service.WriteRegistryValue("Parameters\\MyCustomParameter", 69);

            //
            // Start the service as a kernel driver.
            //

            var KernelService = WindowsServiceKernel.FromService(Service);
            var ReturnStatus = KernelService.TryStartDriver();

            if (ReturnStatus == 0x00000000 ||
                ReturnStatus == 0xC000010E /* STATUS_IMAGE_ALREADY_LOADED */)
            {
                //
                // The service has started.
                //   We will now stop it.
                //

                KernelService.TryStopDriver();
            }

            Console.WriteLine($"[*] CreateService: 0x{ReturnStatus:X8}");

            //
            // Delete the registry key.
            //

            Service.DeleteRegistryKey();
        }

        /// <summary>
        /// Gets the serial number of the specified physical drive.
        /// </summary>
        /// <param name="DriveName">Name of the disk drive.</param>
        public static unsafe string Example_QueryDiskSerial(string DriveName = "PhysicalDrive0")
        {
            var SerialNumber = (string) null;

            //
            // Open the symbolic link created by the disk driver.
            //

            using var DiskDevice = new DeviceIoControl("\\\\.\\" + DriveName);
            DiskDevice.Connect();

            if (!DiskDevice.IsValid)
            {
                Console.WriteLine($"[*] Failed to open '{DriveName}' (error {Marshal.GetLastWin32Error()}). Make sure the program is running elevated.");
                return null;
            }

            //
            // Prepare a request to send to the disk driver.
            //

            var Header = new STORAGE_DESCRIPTOR_HEADER();
            var Query = new STORAGE_PROPERTY_QUERY
            {
                PropertyId = STORAGE_PROPERTY_ID.StorageDeviceProperty,
                QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery
            };

            //
            // Call the disk driver's IRP handler, first to get the size of the descriptor, then to retrieve it.
            //

            if (DiskDevice.TryIoControl(IOCTL_STORAGE_QUERY_PROPERTY, &Query, sizeof(STORAGE_PROPERTY_QUERY), &Header, sizeof(STORAGE_DESCRIPTOR_HEADER)))
            {
                var Allocation = Marshal.AllocHGlobal(Header.Size);

                try
                {
                    if (DiskDevice.TryIoControl(IOCTL_STORAGE_QUERY_PROPERTY, &Query, sizeof(STORAGE_PROPERTY_QUERY), Allocation.ToPointer(), Header.Size, out var ReturnedBytes))
                    {
                        var DeviceDesc = Marshal.PtrToStructure<STORAGE_DEVICE_DESCRIPTOR>(Allocation);

                        if (DeviceDesc.SerialNumberOffset > 0 && DeviceDesc.SerialNumberOffset < ReturnedBytes)
                        {
                            SerialNumber = Marshal.PtrToStringAnsi(IntPtr.Add(Allocation, DeviceDesc.SerialNumberOffset), ReturnedBytes - DeviceDesc.SerialNumberOffset);
                            SerialNumber = SerialNumber.Trim('\0', ' ');
                        }
                    }
                    else
                    {
                        Console.WriteLine("[*] Failed to query for the storage descriptor.");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(Allocation);
                }
            }
            else
            {
                Console.WriteLine("[*] Failed to query for the storage descriptor size.");
            }

            Console.WriteLine($"[*] Disk Serial: {SerialNumber}");
            return SerialNumber;
        }
    }
}
