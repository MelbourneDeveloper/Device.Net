using Device.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net
{
    public interface IHidDevice : IDevice
    {
        Task<ReadReport> ReadReportAsync(CancellationToken cancellationToken = default);
        Task WriteReportAsync(byte[] data, byte? reportId, CancellationToken cancellationToken = default);
        byte? DefaultReportId { get; }
    }
}
