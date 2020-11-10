using Device.Net;

namespace Usb.Net
{
    public interface IUsbDevice : IDevice
    {
        IUsbInterfaceManager UsbInterfaceManager { get; }

        uint SendControlOutTransfer(SetupPacket setupPacket, byte[] buffer);

        uint SendControlInTransfer(WINUSB_SETUP_PACKET setupPacket);
    }
}
