using Device.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net
{
    public interface IHidDeviceHandler
    {
        ConnectedDeviceDefinition ConnectedDeviceDefinition { get; }
        bool? IsReadOnly { get; }
        ushort? ReadBufferSize { get; }
        ushort? WriteBufferSize { get; }
        string DeviceId { get; }

        Task InitializeAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Reads data as a cref="ReadReport"
        /// </summary>
        /// <param name="cancellationToken">Allows you to cancel the operation</param>
        /// <returns>The cref="ReadReport"</returns>
        Task<ReadReport> ReadReportAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes data and allows you to specify the report id
        /// </summary>
        /// <param name="data"></param>
        /// <param name="reportId"></param>
        /// <param name="cancellationToken">Allows you to cancel the operation</param>
        /// <returns></returns>
        Task<uint> WriteReportAsync(byte[] data, byte? reportId, CancellationToken cancellationToken = default);

        public bool IsInitialized { get; }
        public void Close();
    }
}