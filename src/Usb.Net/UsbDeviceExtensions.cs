using Device.Net;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    public static class UsbDeviceExtensions
    {
        public static Task<TransferResult> PerformControlTransferAsync(this IUsbDevice usbDevice, SetupPacket setupPacket, byte[] buffer = null, CancellationToken cancellationToken = default)
            => usbDevice == null ? throw new ArgumentNullException(nameof(usbDevice)) :
            (usbDevice.UsbInterfaceManager.UsbInterfaces.FirstOrDefault() ??
            throw new ControlTransferException("There are no interfaces to send a control transfer to"))
            .PerformControlTransferAsync(setupPacket, buffer, cancellationToken);
    }
}
