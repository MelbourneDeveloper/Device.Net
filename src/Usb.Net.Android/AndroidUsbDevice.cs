using Usb.Net;
using Usb.Net.Android;

namespace Device.Net
{
    public class AndroidUsbDevice : UsbDevice, IUsbDevice
    {
        public AndroidUsbDevice(AndroidUsbDeviceHandler androidUsbDeviceHandler) : base(androidUsbDeviceHandler)
        {
        }
    }
}