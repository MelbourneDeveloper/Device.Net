using System;

namespace Usb.Net.UWP
{
    public class UWPUsbDevice : UsbDevice
    {
        public UWPUsbDevice(UWPUsbDeviceHandler usbDeviceHandler) : base(usbDeviceHandler, usbDeviceHandler?.Logger, usbDeviceHandler?.Tracer)
        {
            if (usbDeviceHandler == null) throw new ArgumentNullException(nameof(usbDeviceHandler));
        }
    }
}
