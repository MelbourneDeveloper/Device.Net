using System;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase : DeviceBase, IDevice
    {
        public string DeviceId { get; set; }

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public abstract void Dispose();
        public abstract Task<bool> GetIsConnectedAsync();
        public abstract Task InitializeAsync();
        public abstract Task<byte[]> ReadAsync();
        public abstract Task WriteAsync(byte[] data);

        protected void RaiseConnected()
        {
            Connected?.Invoke(this, new EventArgs());
        }

        protected void RaiseDisonnected()
        {
            Disconnected?.Invoke(this, new EventArgs());
        }
    }
}
