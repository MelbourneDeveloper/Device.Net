using Device.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net
{
    ///<inheritdoc cref="IDevice"/>
    public interface IHidDevice : IDevice
    {
        /// <summary>
        /// Reads data as a cref="ReadReport"
        /// </summary>
        /// <param name="cancellationToken">Allows you to cancel the operation</param>
        /// <returns>The cref="ReadReport"</returns>
        Task<Report> ReadReportAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes data and allows you to specify the report id
        /// </summary>
        /// <param name="data"></param>
        /// <param name="reportId"></param>
        /// <param name="cancellationToken">Allows you to cancel the operation</param>
        /// <returns></returns>
        Task<uint> WriteReportAsync(byte[] data, byte reportId, CancellationToken cancellationToken = default);
    }
}
