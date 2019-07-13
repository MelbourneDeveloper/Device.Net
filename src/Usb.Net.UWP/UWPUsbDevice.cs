using Device.Net;

namespace Usb.Net.UWP
{
    public class UWPUsbDevice : UsbDevice
    {
        public UWPUsbDevice(UWPUsbDeviceHandler usbDeviceHandler, ILogger logger, ITracer tracer) : base(usbDeviceHandler, logger, tracer)
        {
        }
    }
}
