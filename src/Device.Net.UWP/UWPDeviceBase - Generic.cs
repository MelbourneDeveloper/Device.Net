using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase<T>
    {
        #region Fields
        private bool _IsClosing;
        private bool disposed;
        #endregion

        #region Protected Properties
        protected readonly Observable<byte[]> DataReceivedObservable;
        protected readonly UWPDataReceiver UWPDataReceiver;
        protected T ConnectedDevice { get; private set; }
        public ConnectedDeviceDefinition ConnectedDeviceDefinition { get; protected set; }
        protected ILoggerFactory LoggerFactory { get; private set; }
        protected ILogger<UWPDeviceBase<T>> Logger { get; }
        #endregion

        #region Public
        public string DeviceId { get; }
        #endregion

        #region Public Abstract
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        #endregion

        #region Constructor
        protected UWPDeviceBase(
            string deviceId,
            ILoggerFactory loggerFactory)
        {
            DeviceId = deviceId;
            LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            Logger = loggerFactory.CreateLogger<UWPDeviceBase<T>>();
            DataReceivedObservable = new Observable<byte[]>();
            UWPDataReceiver = new UWPDataReceiver(
                DataReceivedObservable,
                LoggerFactory.CreateLogger<UWPDataReceiver>());
        }
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
        public abstract Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default);
        #endregion

        #region Public Override Properties
        public bool IsInitialized => ConnectedDevice != null;
        #endregion

        #region Public Virtual Methods
        public virtual void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, DeviceId);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, DeviceId);

            Close();

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
                Logger.LogError(ex, Messages.ErrorMessageCantClose, DeviceId, GetType().Name);
            }

            _IsClosing = false;
        }
        #endregion
    }
}
