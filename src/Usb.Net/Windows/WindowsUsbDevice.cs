using Device.Net;
using Microsoft.Extensions.Logging;
using System;

namespace Usb.Net.Windows
{
    [Obsolete(Messages.ObsoleteMessagePlatformSpecificUsbDevice)]
    public class WindowsUsbDevice : UsbDevice
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        public WindowsUsbDevice(string deviceId, ILoggerFactory loggerFactory, ushort? readBufferSize = null, ushort? writeBufferSize = null) : base(deviceId,
            new WindowsUsbInterfaceManager(deviceId,
                loggerFactory,
                readBufferSize,
                writeBufferSize),
            loggerFactory.CreateLogger<WindowsUsbDevice>())
#pragma warning restore CA1062 // Validate arguments of public methods
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        }
    }
}
