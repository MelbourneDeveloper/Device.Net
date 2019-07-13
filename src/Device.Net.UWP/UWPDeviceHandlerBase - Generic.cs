using Device.Net.Exceptions;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceHandlerBase<T> : UWPDeviceHandlerBase, IDeviceHandler
    {
        #region Fields
        private bool _IsClosing;
        #endregion

        #region Protected Properties
        protected T ConnectedDevice { get; private set; }
        protected bool Disposed { get; private set; }
        #endregion

        #region Public Abstract
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        #endregion

        #region Constructor
        protected UWPDeviceHandlerBase(string deviceId, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
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
        public virtual async Task<byte[]> ReadAsync()
        {
            if (IsReading)
            {
                throw new AsyncException(Messages.ErrorMessageReentry);
            }

            //TODO: this should be a semaphore not a lock
            lock (Chunks)
            {
                if (Chunks.Count > 0)
                {
                    var data2 = Chunks[0];
                    Logger?.Log("Received data from device", GetType().Name, null, LogLevel.Information);
                    Chunks.RemoveAt(0);
                    Tracer?.Trace(false, data2);
                    return data2;
                }
            }

            IsReading = true;
            ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();
            var data = await ReadChunkTaskCompletionSource.Task;
            Tracer?.Trace(false, data);
            return data;
        }
        #endregion

        #region Public Override Properties
        public bool IsInitialized => ConnectedDevice != null;
        #endregion

        #region Public Virtual Methods
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;

            Close();
            ReadChunkTaskCompletionSource?.Task?.Dispose();

            GC.SuppressFinalize(this);
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

        #region Finaliser
        ~UWPDeviceHandlerBase()
        {
            Dispose();
        }
        #endregion

    }
}
