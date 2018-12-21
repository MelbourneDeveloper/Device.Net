using System;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase : IDevice
    {
        public string DeviceId { get; internal set; }

        public abstract event EventHandler Connected;
        public abstract event EventHandler Disconnected;

        public abstract void Dispose();
        public abstract Task<bool> GetIsConnectedAsync();
        public abstract Task InitializeAsync();
        public abstract Task<byte[]> ReadAsync();
        public abstract Task WriteAsync(byte[] data);
    }
}
