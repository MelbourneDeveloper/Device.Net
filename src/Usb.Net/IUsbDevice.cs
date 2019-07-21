using Device.Net;

namespace Usb.Net
{
    public interface IUsbDevice : IDevice
    {
         IUsbDeviceHandler UsbDeviceHandler { get; }
    }
}
