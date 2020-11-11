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
        Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);
        Task<ReadResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default);
        byte InterfaceNumber { get; }
        Task ClaimInterface();

        /// <summary>
        /// This is for internal use and should need to be called. This will probably be removed in future versions.
        /// TODO
        /// </summary>
        void RegisterDefaultEndpoints();

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows-hardware/drivers/usbcon/usb-control-transfer
        /// https://docs.microsoft.com/en-us/windows-hardware/drivers/usbcon/how-to-send-a-usb-control-transfer--uwp-app-
        /// TODO: Remove the buffer parameter?
        /// Can we just create the buffer in the method?
        /// Or, do we sometimes need to send data here?
        /// </summary>
        Task<ControlTransferResult> SendControlTransferAsync(SetupPacket setupPacket, byte[] buffer = null, CancellationToken cancellationToken = default);
    }
}