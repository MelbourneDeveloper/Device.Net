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
        Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default);
        Task<uint> WriteAsync(byte[] bytes, CancellationToken cancellationToken = default);
        public bool IsInitialized { get; }
        public void Close();
    }
}