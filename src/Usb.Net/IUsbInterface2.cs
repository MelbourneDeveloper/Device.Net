using Device.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    public interface IUsbInterface2
    {
        Task<TransferResult> ReadFromEndpointAsync(IUsbInterfaceEndpoint endpoint, uint bufferLength, CancellationToken cancellationToken = default);
        Task<uint> WriteToEndpointAsync(IUsbInterfaceEndpoint endpoint, byte[] data);
        Task<TransferResult> ControlTransferAsync(SetupPacket setupPacket, byte[] data);
        void Close();
    }
}
