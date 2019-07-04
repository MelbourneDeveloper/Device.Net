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
        IUsbInterfaceEndpoint InterruptEndpoint { get; set; }

        Task WriteAsync(byte[] data);
        Task<byte[]> ReadAsync(uint bufferLength);

        Task<byte[]> ReadInterruptAsync(uint bufferLength, uint timeout);
    }
}