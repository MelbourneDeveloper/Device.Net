using Device.Net;

namespace Usb.Net
{
    ///<inheritdoc cref="IDevice"/>
    public interface IUsbDevice : IDevice
    {
        IUsbInterfaceManager UsbInterfaceManager { get; }
    }
}
