using Device.Net;
using Microsoft.Extensions.Logging;
using System;

namespace Usb.Net.Windows
{
    [Obsolete(Messages.ObsoleteMessagePlatformSpecificUsbDevice)]
    public class WindowsUsbDevice : UsbDevice
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        public WindowsUsbDevice(string deviceId, ILoggerFactory loggerFactory, ITracer tracer, ushort? readBufferSize, ushort? writeBufferSize) : base(deviceId, new WindowsUsbInterfaceManager(deviceId, loggerFactory.CreateLogger(nameof(WindowsUsbInterfaceManager)), tracer, readBufferSize, writeBufferSize), loggerFactory.CreateLogger(nameof(WindowsUsbDevice)), tracer)
#pragma warning restore CA1062 // Validate arguments of public methods
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        }
    }
}
