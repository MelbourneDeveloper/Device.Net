using Device.Net;
using Microsoft.Extensions.Logging;
using System;

namespace Usb.Net.Windows
{
    [Obsolete(Messages.ObsoleteMessagePlatformSpecificUsbDevice)]
    public class WindowsUsbDevice : UsbDevice
    {
        public WindowsUsbDevice(string deviceId, ILogger logger, ITracer tracer, ushort? readBufferSize, ushort? writeBufferSize) : base(deviceId, new WindowsUsbInterfaceManager(deviceId, logger, tracer, readBufferSize, writeBufferSize), logger, tracer)
        {
        }
    }
}
