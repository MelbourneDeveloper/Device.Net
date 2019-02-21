using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase<T> : UWPDeviceBase, IDevice
    {
        #region Fields
        private bool _IsClosing;
        private bool disposed = false;
        #endregion

        #region Protected Properties
        protected T ConnectedDevice { get; private set; }
        #endregion

        #region Constructor
        protected UWPDeviceBase()
        {

        }

        protected UWPDeviceBase(string deviceId)
        {
            DeviceId = deviceId;
        }
        #endregion

        #region Protected Methods
        protected async Task GetDeviceAsync(string id)
        {
            var asyncOperation = FromIdAsync(id);
            var task = asyncOperation.AsTask();
            ConnectedDevice = await task;
        }
        #endregion

        #region Protected Abstract Methods
        protected abstract IAsyncOperation<T> FromIdAsync(string id);
        #endregion

        #region Public Overrides
        public override async Task<byte[]> ReadAsync()
        {
            if (_IsReading)
            {
                throw new Exception("Reentry");
            }

            lock (Chunks)
            {
                if (Chunks.Count > 0)
                {
                    var retVal = Chunks[0];
                    Tracer?.Trace(false, retVal);
                    Chunks.RemoveAt(0);
                    return retVal;
                }
            }

            _IsReading = true;
            ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();
            return await ReadChunkTaskCompletionSource.Task;
        }
        #endregion

        #region Public Override Properties
        public override bool IsInitialized => ConnectedDevice != null;
        #endregion

        #region Public Virtual Methods
        public override sealed void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Close();
            ReadChunkTaskCompletionSource?.Task?.Dispose();

            base.Dispose();
        }

        public void Close()
        {
            if (_IsClosing) return;

            _IsClosing = true;

            try
            {
                if (ConnectedDevice is IDisposable disposable) disposable.Dispose();
                ConnectedDevice = default(T);
            }
            catch (Exception ex)
            {
                Log("Error disposing", ex);
            }

            _IsClosing = false;
        }
        #endregion
    }
}
