using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase : DeviceBase, IDevice
    {
        #region Fields
        protected TaskCompletionSource<byte[]> _TaskCompletionSource = null;
        protected readonly Collection<byte[]> _Chunks = new Collection<byte[]>();
        protected bool _IsReading;
        #endregion

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

        protected  void HandleDataReceived(byte[] bytes)
        {
            if (!_IsReading)
            {
                lock (_Chunks)
                {
                    _Chunks.Add(bytes);
                }
            }
            else
            {
                _IsReading = false;
                _TaskCompletionSource.SetResult(bytes);
            }
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

        #region Public Overrides
        public override async Task<bool> GetIsConnectedAsync()
        {
            return _ConnectedDevice != null;
        }
        #endregion
    }
}
