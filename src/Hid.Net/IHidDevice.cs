using Device.Net;
using System.Threading.Tasks;

namespace Hid.Net
{
    public interface IHidDevice : IDevice
    {
        Task<ReadReport> ReadReportAsync();
        Task WriteReportAsync(byte[] data, byte? reportId);
        byte? DefaultReportId { get; }
    }
}
