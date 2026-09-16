namespace Driver.NET.Services
{
    using System.Linq;

    /// <summary>
    /// Represents a Windows service, identified by its configuration key in the registry.
    /// </summary>
    public partial class WindowsService
    {
        /// <summary>
        /// The registry path of the local machine hive.
        /// </summary>
        private const string LocalMachineHive = "HKEY_LOCAL_MACHINE";

        /// <summary>
        /// Gets the registry path of this service.
        /// </summary>
        public string RegistryPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the registry path of this service relative to the local machine hive path.
        /// </summary>
        public string RegistryPathRelativeToLocalMachine => this.RegistryPath?.Substring(LocalMachineHive.Length + 1);

        /// <summary>
        /// Gets the registry path of this service in the native (object manager) form used by the kernel,
        /// for example '\Registry\Machine\SYSTEM\CurrentControlSet\Services\MyDriver'.
        /// </summary>
        public string NativeRegistryPath => this.RegistryPath == null ? null : @"\Registry\Machine\" + this.RegistryPathRelativeToLocalMachine;

        /// <summary>
        /// Gets the name of this service.
        /// </summary>
        public string ServiceName => this.RegistryPath?.Split('\\').LastOrDefault();

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsService"/> class.
        /// </summary>
        internal WindowsService()
        {
            // ...
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.RegistryPath;
        }
    }
}
