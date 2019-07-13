namespace Usb.Net.UWP
{
    public class UWPUsbDevice : UsbDevice
    {
        public UWPUsbDevice(UWPUsbDeviceHandler usbDeviceHandler) : base(usbDeviceHandler, usbDeviceHandler.Logger, usbDeviceHandler.Tracer)
        {
        }
    }
}
