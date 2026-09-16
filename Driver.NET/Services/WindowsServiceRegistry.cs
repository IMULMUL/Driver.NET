namespace Driver.NET.Services
{
    using System;

    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;

    public partial class WindowsService
    {
        /// <summary>
        /// Opens a handle to this service's registry key, or returns <c>null</c> if the key does not exist.
        /// </summary>
        public SafeRegistryHandle OpenRegistryHandle()
        {
            return this.OpenRegistryKey()?.Handle;
        }

        /// <summary>
        /// Opens this service's registry key wrapped in a <see cref="RegistryKey"/> instance, or returns <c>null</c> if the key does not exist.
        /// </summary>
        public RegistryKey OpenRegistryKey()
        {
            return Registry.LocalMachine.OpenSubKey(this.RegistryPathRelativeToLocalMachine);
        }

        /// <summary>
        /// Creates the registry key for this service.
        /// </summary>
        public void CreateRegistryKey()
        {
            Registry.LocalMachine.CreateSubKey(this.RegistryPathRelativeToLocalMachine)?.Dispose();
        }

        /// <summary>
        /// Deletes the registry key for this service, if it exists.
        /// </summary>
        public void DeleteRegistryKey()
        {
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(this.RegistryPathRelativeToLocalMachine, false);
            }
            catch (Exception)
            {
                // ...
            }
        }

        /// <summary>
        /// Reads and return a registry value from the current service's registry key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="ValueName">The name of the value, optionally prefixed by a sub-key path (e.g. 'Parameters\MyValue').</param>
        /// <param name="DefaultValue">The value returned when the value or its key does not exist.</param>
        public T ReadRegistryValue<T>(string ValueName, T DefaultValue = default)
        {
            var KeyPath = this.ResolveValuePath(ValueName, out _, out var RealValueName);
            var Value = Registry.GetValue(KeyPath, RealValueName, DefaultValue);

            return Value == null ? DefaultValue : (T) Value;
        }

        /// <summary>
        /// Writes a registry value to the current service's registry key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="ValueName">The name of the value, optionally prefixed by a sub-key path (e.g. 'Parameters\MyValue').</param>
        /// <param name="Value">The value.</param>
        /// <param name="ValueKind">The type of the value in registry.</param>
        public void WriteRegistryValue<T>(string ValueName, T Value, RegistryValueKind ValueKind = RegistryValueKind.Unknown)
        {
            var KeyPath = this.ResolveValuePath(ValueName, out var SubKeyName, out var RealValueName);

            //
            // Make sure the sub-key exists.
            //

            if (SubKeyName != null)
            {
                using (var ServiceKey = Registry.LocalMachine.OpenSubKey(this.RegistryPathRelativeToLocalMachine, true))
                {
                    if (ServiceKey == null)
                    {
                        throw new InvalidOperationException("The registry key of the service does not exist.");
                    }

                    ServiceKey.CreateSubKey(SubKeyName)?.Dispose();
                }
            }

            Registry.SetValue(KeyPath, RealValueName, Value, ValueKind);
        }

        /// <summary>
        /// Splits a value name that may be prefixed by a sub-key path (e.g. 'Parameters\MyValue')
        /// into the sub-key path, the actual value name, and the full registry path of the key holding the value.
        /// </summary>
        /// <param name="ValueName">The name of the value, optionally prefixed by a sub-key path.</param>
        /// <param name="SubKeyName">The sub-key path relative to the service's key, or <c>null</c> if there is none.</param>
        /// <param name="RealValueName">The actual name of the value.</param>
        /// <returns>The full registry path of the key holding the value.</returns>
        private string ResolveValuePath(string ValueName, out string SubKeyName, out string RealValueName)
        {
            if (string.IsNullOrEmpty(ValueName))
            {
                throw new ArgumentNullException(nameof(ValueName), "The registry value name is null or empty.");
            }

            //
            // Convert all forward slashes '/' to backward slashes '\' and make sure the name does not end with a slash.
            //

            ValueName = ValueName.Replace('/', '\\').TrimEnd('\\');

            //
            // Parse the sub-key name and the real value name.
            //

            var Separator = ValueName.LastIndexOf('\\');

            if (Separator <= 0)
            {
                SubKeyName = null;
                RealValueName = Separator < 0 ? ValueName : ValueName.Substring(1);
                return this.RegistryPath;
            }

            SubKeyName = ValueName.Substring(0, Separator);
            RealValueName = ValueName.Substring(Separator + 1);
            return this.RegistryPath + "\\" + SubKeyName;
        }
    }
}
