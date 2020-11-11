using System;

namespace Usb.Net.Windows
{
    public static class WindowsUsbExtensions
    {
        public static WINUSB_SETUP_PACKET ToWindowsSetupPacket(this SetupPacket setupPacket)
            => setupPacket == null ? throw new ArgumentNullException(nameof(setupPacket)) : new WINUSB_SETUP_PACKET
            {
                Index = setupPacket.Length,
                Length = setupPacket.Length,
                Request = setupPacket.Request,
                RequestType = setupPacket.RequestType.ToByte(),
                Value = setupPacket.Value
            };
    }
}
