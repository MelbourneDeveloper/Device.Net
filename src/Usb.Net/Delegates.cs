using Device.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    public delegate Task<TransferResult> PerformControlTransferAsync(SetupPacket setupPacket, byte[] buffer, CancellationToken cancellationToken = default);

}
