using Device.Net;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    public static class UsbDeviceExtensions
    {
        public static Task<ReadResult> SendControlTransferAsync(this IUsbDevice usbDevice, SetupPacket setupPacket, byte[] buffer = null, CancellationToken cancellationToken = default)
            => usbDevice == null ? throw new ArgumentNullException(nameof(usbDevice)) :
            (usbDevice.UsbInterfaceManager.UsbInterfaces.FirstOrDefault() ??
            throw new Exception("There are no interfaces to send a control transfer to"))
            .SendControlTransferAsync(setupPacket, buffer, cancellationToken);
    }
}
