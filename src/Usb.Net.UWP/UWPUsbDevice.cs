using System;

namespace Usb.Net.UWP
{
    public class UWPUsbDevice : UsbDevice
    {
        public UWPUsbDevice(UWPUsbInterfaceManager usbDeviceHandler) : base(usbDeviceHandler.DeviceId, usbDeviceHandler, usbDeviceHandler?.Logger, usbDeviceHandler?.Tracer)
        {
            if (usbDeviceHandler == null) throw new ArgumentNullException(nameof(usbDeviceHandler));
        }
    }
}
