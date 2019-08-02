using Device.Net;
using System;

namespace Usb.Net.Android
{
    [Obsolete(Messages.ObsoleteMessagePlatformSpecificUsbDevice)]
    public class AndroidUsbDevice : UsbDevice
    {
        public AndroidUsbDevice(AndroidUsbInterfaceManager androidUsbInterfaceManager, ILogger logger, ITracer tracer) : base(androidUsbInterfaceManager, logger, tracer)
        {
        }
    }
}