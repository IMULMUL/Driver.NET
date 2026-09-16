namespace Driver.NET.DeviceIoControl
{
    using System;

    public partial class DeviceIoControl
    {
        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        public unsafe bool TryIoControl(uint Ioctl)
        {
            return NtDeviceIoControl(this.Handle, Ioctl, null, 0, null, 0, out _, IntPtr.Zero);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <typeparam name="TInput">The type of the input buffer.</typeparam>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        public unsafe bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer)
            where TInput : unmanaged
        {
            return this.TryIoControl(Ioctl, InputBuffer, sizeof(TInput));
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <typeparam name="TInput">The type of the input buffer.</typeparam>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        public unsafe bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer, int InputBufferSize)
            where TInput : unmanaged
        {
            return NtDeviceIoControl(this.Handle, Ioctl, &InputBuffer, InputBufferSize, null, 0, out _, IntPtr.Zero);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <typeparam name="TInput">The type of the input buffer.</typeparam>
        /// <typeparam name="TOutput">The type of the output buffer.</typeparam>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        public unsafe bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, out TOutput OutputBuffer)
            where TInput : unmanaged
            where TOutput : unmanaged
        {
            return this.TryIoControl(Ioctl, InputBuffer, sizeof(TInput), out OutputBuffer, sizeof(TOutput));
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <typeparam name="TInput">The type of the input buffer.</typeparam>
        /// <typeparam name="TOutput">The type of the output buffer.</typeparam>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        public unsafe bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, int InputBufferSize, out TOutput OutputBuffer, int OutputBufferSize)
            where TInput : unmanaged
            where TOutput : unmanaged
        {
            OutputBuffer = default;

            fixed (TOutput* RealOutputBuffer = &OutputBuffer)
            {
                return NtDeviceIoControl(this.Handle, Ioctl, &InputBuffer, InputBufferSize, RealOutputBuffer, OutputBufferSize, out _, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <typeparam name="TInput">The type of the input buffer.</typeparam>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        public unsafe bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize)
            where TInput : unmanaged
        {
            return NtDeviceIoControl(this.Handle, Ioctl, &InputBuffer, InputBufferSize, OutputBuffer, OutputBufferSize, out _, IntPtr.Zero);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        public unsafe bool TryIoControl(uint Ioctl, void* InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize)
        {
            return this.TryIoControl(Ioctl, InputBuffer, InputBufferSize, OutputBuffer, OutputBufferSize, out _);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        /// <param name="ReturnedBytes">The number of bytes written to the output buffer by the driver.</param>
        public unsafe bool TryIoControl(uint Ioctl, void* InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize, out int ReturnedBytes)
        {
            return NtDeviceIoControl(this.Handle, Ioctl, InputBuffer, InputBufferSize, OutputBuffer, OutputBufferSize, out ReturnedBytes, IntPtr.Zero);
        }
    }
}
