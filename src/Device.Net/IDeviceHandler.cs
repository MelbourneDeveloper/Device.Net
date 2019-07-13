using System;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceHandler : IDisposable
    {
        ushort WriteBufferSize { get; }
        ushort ReadBufferSize { get; }
        bool IsInitialized { get; }
        Task InitializeAsync();
        void Close();
    }
}