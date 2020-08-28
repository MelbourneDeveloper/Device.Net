using Device.Net;
using System;
using System.Collections.Generic;
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
        uint ControlTransfer(ISetupPacket setupPacket, byte[] buffer); //TODO: setup packet is defined differently between libUsb and WinUsb (also possibly convert to async)
        Task WriteAsync(byte[] data);
        Task<ReadResult> ReadAsync(uint bufferLength);
        byte InterfaceNumber { get; }
        Task ClaimInterface();

        /// <summary>
        /// This is for internal use and should need to be called. This will probably be removed in future versions.
        /// TODO
        /// </summary>
        void RegisterDefaultEndpoints();
    }
}