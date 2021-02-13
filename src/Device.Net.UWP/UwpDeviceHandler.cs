using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

#nullable enable

namespace Device.Net.UWP
{
    public abstract class UwpDeviceHandler<T>
    {
        #region Fields
        private bool _IsClosing;
        private bool disposed;
        #endregion

        #region Protected Properties
        protected readonly IDataReceiver DataReceiver;
        protected T? ConnectedDevice { get; private set; }
        public ConnectedDeviceDefinition? ConnectedDeviceDefinition { get; protected set; }
        protected ILoggerFactory LoggerFactory { get; private set; }
        protected ILogger<UwpDeviceHandler<T>> Logger { get; }
        #endregion

        #region Public
        public string DeviceId { get; }
        #endregion

        #region Constructor
        protected UwpDeviceHandler(
            string deviceId,
            IDataReceiver dataReceiver,
            ILoggerFactory? loggerFactory)
        {
            DeviceId = deviceId;
            LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            Logger = loggerFactory.CreateLogger<UwpDeviceHandler<T>>();
            DataReceiver = dataReceiver;
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

            DataReceiver.Dispose();

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
