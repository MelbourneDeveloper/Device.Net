using System;
using System.Linq;

namespace Usb.Net
{
    public static class UsbDeviceExtensions
    {
        public static uint SendControlOutTransfer(this IUsbDevice usbDevice, SetupPacket setupPacket, byte[] buffer)
            => usbDevice == null ? throw new ArgumentNullException(nameof(usbDevice)) : (usbDevice.UsbInterfaceManager.UsbInterfaces.FirstOrDefault() ?? throw new Exception("There are no interfaces to send a control transfer to")).SendControlOutTransfer(setupPacket, buffer);
    }
}
