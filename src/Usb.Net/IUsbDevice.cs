using Device.Net;

namespace Usb.Net
{
    public interface IUsbDevice : IDevice
    {
        IUsbInterfaceManager UsbInterfaceManager { get; }
    }
}
