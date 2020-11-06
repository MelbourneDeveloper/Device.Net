using Device.Net;

namespace Usb.Net
{
    public interface IUsbDevice : IDevice
    {
        IUsbInterfaceManager UsbInterfaceManager { get; }

        uint SendControlOutTransfer(object setupPacket, byte[] buffer);

        uint SendControlInTransfer(object setupPacket);
    }
}
