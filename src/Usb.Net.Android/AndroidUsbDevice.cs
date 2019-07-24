using Device.Net;

namespace Usb.Net.Android
{
    public class AndroidUsbDevice : UsbDevice
    {
        public AndroidUsbDevice(AndroidUsbInterfaceManager androidUsbInterfaceManager, ILogger logger, ITracer tracer) : base(androidUsbInterfaceManager, logger, tracer)
        {
        }
    }
}