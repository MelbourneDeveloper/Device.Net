using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase : DeviceBase
    {
        #region Fields
        protected TaskCompletionSource<byte[]> _TaskCompletionSource = null;
        protected readonly Collection<byte[]> _Chunks = new Collection<byte[]>();
        protected bool _IsReading;
        #endregion

        #region Public Properties
        public string DeviceId { get; set; }
        #endregion

        #region Protected Methods
        protected void HandleDataReceived(byte[] bytes)
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
        public abstract Task<bool> GetIsConnectedAsync();
        public abstract Task InitializeAsync();
        #endregion
    }
}
