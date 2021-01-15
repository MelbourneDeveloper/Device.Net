using Device.Net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net
{
    public interface IHidDeviceHandler : IDisposable
    {
        ConnectedDeviceDefinition ConnectedDeviceDefinition { get; }
        bool? IsReadOnly { get; }
        ushort? ReadBufferSize { get; }
        ushort? WriteBufferSize { get; }
        string DeviceId { get; }
        void Initialize();
        Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default);
        Task<uint> WriteAsync(byte[] bytes, CancellationToken cancellationToken = default);
    }
}