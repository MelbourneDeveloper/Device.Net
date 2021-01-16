using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary>
    /// Base class for all devices
    /// </summary>
    public abstract class DeviceBase : IDisposable
    {
        #region Fields
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        #endregion

        #region Protected Properties
        protected ILogger Logger { get; }
        protected ILoggerFactory LoggerFactory { get; }
        #endregion

        #region Public Abstract Properties
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        public abstract bool IsInitialized { get; }
        #endregion

        #region Public Properties
        public string DeviceId { get; }
        #endregion

        #region Constructor
        protected DeviceBase(
            string deviceId,
            ILoggerFactory loggerFactory = null,
            ILogger logger = null)
        {
            DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            Logger = logger ?? NullLogger.Instance;
            LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }
        #endregion

        #region Public Abstract Methods
        //TODO: Why are these here?

        public abstract Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default);
        public abstract Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default);
        #endregion

        #region Public Methods
        public virtual Task Flush(CancellationToken cancellationToken = default) => throw new NotImplementedException(Messages.ErrorMessageFlushNotImplemented);

        public async Task<TransferResult> WriteAndReadAsync(byte[] writeBuffer, CancellationToken cancellationToken = default)
        {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            await _WriteAndReadLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            using var logScope = Logger.BeginScope("DeviceId: {deviceId} Call: {call} Write Buffer Length: {writeBufferLength}", DeviceId, nameof(WriteAndReadAsync), writeBuffer.Length);

            try
            {
                _ = await WriteAsync(writeBuffer, cancellationToken).ConfigureAwait(false);
                var retVal = await ReadAsync(cancellationToken).ConfigureAwait(false);
                Logger.LogInformation(Messages.SuccessMessageWriteAndReadCalled);
                return retVal;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Messages.ErrorMessageReadWrite);
                throw;
            }
            finally
            {
                _ = _WriteAndReadLock.Release();
            }
        }

        /// <summary> 
        /// Many Hid devices on Windows have a buffer size that is one byte larger than the logical buffer size. For compatibility with other platforms etc. we need to remove the first byte. See DataHasExtraByte
        /// </summary> 
        public static byte[] RemoveFirstByte(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var length = bytes.Length - 1;
            var retVal = new byte[length];

            Array.Copy(bytes, 1, retVal, 0, length);

            return retVal;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, DeviceId);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, DeviceId);

            _WriteAndReadLock.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Public Static Methods
        public static ConnectedDeviceDefinition GetDeviceDefinitionFromWindowsDeviceId(
            string deviceId,
            DeviceType deviceType,
            ILogger logger,
            Guid? classGuid = null)
        {
            uint? vid = null;
            uint? pid = null;
            try
            {
                vid = GetNumberFromDeviceId(deviceId, "vid_");
                pid = GetNumberFromDeviceId(deviceId, "pid_");
            }
#pragma warning disable CA1031 
            catch (Exception ex)
#pragma warning restore CA1031 
            {
                //If anything goes wrong here, log it and move on. 
                (logger ?? NullLogger.Instance).LogError(ex, "Error {errorMessage} Area: {area}", ex.Message, nameof(GetDeviceDefinitionFromWindowsDeviceId));
            }

            return new ConnectedDeviceDefinition(deviceId, deviceType, vid, pid, classGuid: classGuid);
        }
        #endregion

        #region Private Static Methods
        private static uint GetNumberFromDeviceId(string deviceId, string searchString)
        {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));

            var indexOfSearchString = deviceId.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);
            string hexString = null;
            if (indexOfSearchString > -1)
            {
                hexString = deviceId.Substring(indexOfSearchString + searchString.Length, 4);
            }
#pragma warning disable CA1305 // Specify IFormatProvider
            var numberAsInteger = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
#pragma warning restore CA1305 // Specify IFormatProvider
            return numberAsInteger;
        }
        #endregion
    }
}
