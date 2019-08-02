using Device.Net;
using System;

namespace Usb.Net.UWP
{
    [Obsolete(Messages.ObsoleteMessagePlatformSpecificUsbDevice)]
    public class UWPUsbDevice : UsbDevice
    {
        public UWPUsbDevice(UWPUsbInterfaceManager usbDeviceHandler) : base(usbDeviceHandler, usbDeviceHandler?.Logger, usbDeviceHandler?.Tracer)
        {
            if (usbDeviceHandler == null) throw new ArgumentNullException(nameof(usbDeviceHandler));
        }
    }
}
