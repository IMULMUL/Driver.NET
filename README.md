# Driver.NET [![Driver.NET](https://img.shields.io/nuget/v/Driver.NET.svg)](https://www.nuget.org/packages/Driver.NET/) [![Driver.NET](https://img.shields.io/nuget/dt/Driver.NET.svg)](https://www.nuget.org/packages/Driver.NET/)
``Driver.NET`` is a powerful, simple and lightweight library used to create services and load/communicate with kernel drivers on Windows.

The solution is split in two libraries:

| Library | Purpose |
| --- | --- |
| `Driver.NET` | Create and configure a service in the registry, then load/unload it as a kernel driver (`NtLoadDriver` / `NtUnloadDriver`). |
| `Driver.NET.DeviceIoControl` | Open the symbolic link of a device and send I/O control requests (IOCTL) to the driver behind it. |

# Supported frameworks
- .NET 10
- .NET Framework 4.8

Both libraries are Windows-only. Loading and unloading drivers requires an elevated process holding `SeLoadDriverPrivilege`.

# Usage
## Send an IOCTL to a device
```csharp
using Driver.NET.DeviceIoControl;

using var Device = new DeviceIoControl("\\\\.\\MyDevice");
Device.Connect();

if (Device.IsValid)
{
    var Request = new MY_REQUEST { Value = 42 };

    if (Device.TryIoControl(IOCTL_MY_REQUEST, Request, out MY_RESPONSE Response))
    {
        // Use the response.
    }
}
```

## Create, load and unload a kernel driver service
```csharp
using Driver.NET.Services;

var Service = WindowsService.FromServiceName("mydriver");
Service.CreateRegistryKey();
Service.WriteRegistryValue("ImagePath", "System32\\drivers\\mydriver.sys");
Service.WriteRegistryValue("Type", 1);
Service.WriteRegistryValue("ErrorControl", 1);
Service.WriteRegistryValue("Start", 1);
Service.WriteRegistryValue("Parameters\\MyCustomParameter", 69);

var KernelService = WindowsServiceKernel.FromService(Service);
var Status = KernelService.TryStartDriver(); // NTSTATUS, 0 on success.

KernelService.TryStopDriver();
Service.DeleteRegistryKey();
```

A complete example that retrieves the serial number of a disk by talking to the `disk.sys` driver through `\\.\PhysicalDrive0` is available in the repository, in the *Driver.NET.Example* project.
