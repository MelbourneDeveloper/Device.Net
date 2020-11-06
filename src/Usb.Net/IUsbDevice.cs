using Device.Net;

namespace Usb.Net
{
    public interface IUsbDevice : IDevice
    {
        IUsbInterfaceManager UsbInterfaceManager { get; }

        uint SendControlOutTransfer(ISetupPacket setupPacket, byte[] buffer);

        uint SendControlInTransfer(ISetupPacket setupPacket);
    }
}
