using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase<T> : UWPDeviceBase, IDeviceHandler
    {
        #region Fields
        private bool _IsClosing;
        private bool disposed;
        #endregion

        #region Protected Properties
        protected T ConnectedDevice { get; private set; }
        public ConnectedDeviceDefinition ConnectedDeviceDefinition { get; protected set; }
        protected ILoggerFactory LoggerFactory { get; private set; }
        #endregion

        #region Public Abstract
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        #endregion

        #region Constructor
        protected UWPDeviceBase(string deviceId, ILoggerFactory loggerFactory, ILogger logger) : base(deviceId, logger) => LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        #endregion

        #region Protected Methods
        protected async Task GetDeviceAsync(string id, CancellationToken cancellationToken = default)
        {
            var asyncOperation = FromIdAsync(id);
            var task = asyncOperation.AsTask(cancellationToken);
            ConnectedDevice = await task;
        }
        #endregion

        #region Protected Abstract Methods
        protected abstract IAsyncOperation<T> FromIdAsync(string id);
        #endregion

        #region Public Overrides
        public virtual async Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default)
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
                    Logger?.LogDebug("Received data from device Region: {region}", GetType().Name);
                    Chunks.RemoveAt(0);
                    Logger.LogTrace(new Trace(false, data2));
                    return data2;
                }
            }

            IsReading = true;

            ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();

            //Cancel the completion source if the token is canceled
            using (cancellationToken.Register(() => ReadChunkTaskCompletionSource.TrySetCanceled()))
            {
                await ReadChunkTaskCompletionSource.Task;
            }

            var data = await ReadChunkTaskCompletionSource.Task;
            Logger.LogTrace(new Trace(false, data));
            return data;
        }
        #endregion

        #region Public Override Properties
        public bool IsInitialized => ConnectedDevice != null;
        #endregion

        #region Public Virtual Methods
        public virtual void Dispose()
        {
            if (disposed) return;
            disposed = true;

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
                ConnectedDevice = default;
            }
#pragma warning disable CA1031 
            catch (Exception ex)
#pragma warning restore CA1031 
            {
                //Log and move on
                Logger?.LogError(ex, Messages.ErrorMessageCantClose, DeviceId, GetType().Name);
            }

            _IsClosing = false;
        }
        #endregion

        #region Finaliser
        ~UWPDeviceBase()
        {
            Dispose();
        }
        #endregion

    }
}
