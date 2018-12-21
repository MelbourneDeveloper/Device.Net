using System;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase : DeviceBase, IDevice
    {
        #region Public Properties
        public string DeviceId { get; set; }
        #endregion

        #region Events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        #endregion

        #region Protected Methods
        protected void RaiseConnected()
        {
            Connected?.Invoke(this, new EventArgs());
        }

        protected void RaiseDisonnected()
        {
            Disconnected?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Public Abstract Methods
        public abstract void Dispose();
        public abstract Task<bool> GetIsConnectedAsync();
        public abstract Task InitializeAsync();
        public abstract Task<byte[]> ReadAsync();
        public abstract Task WriteAsync(byte[] data);
        #endregion
    }

    public abstract class UWPDeviceBase<T> : UWPDeviceBase
    {
        #region Protected Properties
        protected T _ConnectedDevice;
        #endregion
    }
}
