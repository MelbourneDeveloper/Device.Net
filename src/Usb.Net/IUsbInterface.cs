using Device.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    public interface IUsbInterface : IDisposable
    {
        IUsbInterfaceEndpoint ReadEndpoint { get; set; }
        IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; }
        IUsbInterfaceEndpoint WriteEndpoint { get; set; }
        IUsbInterfaceEndpoint InterruptWriteEndpoint { get; set; }
        IUsbInterfaceEndpoint InterruptReadEndpoint { get; set; }
        //TODO: Remove these. They should come from the endpoint... or be specified there
        ushort ReadBufferSize { get; }
        ushort WriteBufferSize { get; }
        Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default);
        Task<TransferResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default);
        byte InterfaceNumber { get; }
        Task ClaimInterface();

        /// <summary>
        /// This is for internal use and should need to be called. This will probably be removed in future versions.
        /// TODO
        /// </summary>
        void RegisterDefaultEndpoints();

        /// <summary>
        /// Performs a USB Control Transfer
        /// https://docs.microsoft.com/en-us/windows-hardware/drivers/usbcon/usb-control-transfer
        /// https://docs.microsoft.com/en-us/windows-hardware/drivers/usbcon/how-to-send-a-usb-control-transfer--uwp-app-
        /// </summary>
        Task<TransferResult> PerformControlTransferAsync(SetupPacket setupPacket, byte[]? buffer = null, CancellationToken cancellationToken = default);
    }
}