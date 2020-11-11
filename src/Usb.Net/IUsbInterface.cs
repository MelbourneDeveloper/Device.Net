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
        uint SendControlOutTransfer(ISetupPacket setupPacket, byte[] buffer); //TODO: setup packet is defined differently between libUsb and WinUsb (also possibly convert to async)
        uint SendControlInTransfer(ISetupPacket setupPacket); //TODO: setup packet is defined differently between libUsb and WinUsb (also possibly convert to async)
        byte InterfaceNumber { get; }
        Task ClaimInterface();

        /// <summary>
        /// This is for internal use and should need to be called. This will probably be removed in future versions.
        /// TODO
        /// </summary>
        void RegisterDefaultEndpoints();
    }
}