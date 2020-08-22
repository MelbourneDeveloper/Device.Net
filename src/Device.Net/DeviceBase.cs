using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    public abstract class DeviceBase : IDisposable
    {
        #region Fields
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        #endregion

        #region Protected Properties
        protected ILogger Logger { get; }
        #endregion

        #region Public Abstract Properties
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        public abstract bool IsInitialized { get; }
        #endregion

        #region Public Properties
        public ConnectedDeviceDefinitionBase ConnectedDeviceDefinition { get; set; }
        public string DeviceId { get; }
        public ITracer Tracer { get; }
        #endregion

        #region Constructor
        protected DeviceBase(string deviceId, ILogger logger, ITracer tracer)
        {
            DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            Tracer = tracer;
            Logger = logger;
        }
        #endregion

        #region Public Abstract Methods
        //TODO: Why are these here?

        public abstract Task<ReadResult> ReadAsync(CancellationToken cancellationToken = default);
        public abstract Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);
        #endregion

        #region Public Methods
        public virtual Task Flush(CancellationToken cancellationToken = default) => throw new NotImplementedException(Messages.ErrorMessageFlushNotImplemented);

        public async Task<ReadResult> WriteAndReadAsync(byte[] writeBuffer, CancellationToken cancellationToken = default)
        {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            await _WriteAndReadLock.WaitAsync(cancellationToken);

            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call} Write Buffer Length: {writeBufferLength}", DeviceId, nameof(WriteAndReadAsync), writeBuffer.Length);
                await WriteAsync(writeBuffer, cancellationToken);
                var retVal = await ReadAsync(cancellationToken);
                Logger?.LogInformation(Messages.SuccessMessageWriteAndReadCalled);
                return retVal;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageReadWrite);
                throw;
            }
            finally
            {
                logScope?.Dispose();
                _WriteAndReadLock.Release();
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
            if (disposed) return;

            disposed = true;

            _WriteAndReadLock.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Finalizer
        ~DeviceBase()
        {
            Dispose();
        }
        #endregion
    }
}
