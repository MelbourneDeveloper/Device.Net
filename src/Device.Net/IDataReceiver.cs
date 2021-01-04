using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary>
    /// Receives data from some source
    /// </summary>
    public interface IDataReceiver : IDisposable
    {
        /// <summary>
        /// Read from the source or wait for data to appear for reading
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Manually set the data that appears at the source
        /// </summary>
        /// <param name="bytes"></param>
        void DataReceived(byte[] bytes);
    }
}