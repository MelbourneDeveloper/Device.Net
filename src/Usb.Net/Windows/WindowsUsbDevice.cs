using Device.Net;

namespace Usb.Net.Windows
{
    public class WindowsUsbDevice : UsbDevice
    {
        public WindowsUsbDevice(string deviceId, ILogger logger, ITracer tracer) : base(new WindowsUsbDeviceHandler(deviceId, logger, tracer))
        {
        }
    }
}
