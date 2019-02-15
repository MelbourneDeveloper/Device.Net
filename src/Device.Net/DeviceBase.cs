using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    public abstract class DeviceBase
    {
        #region Fields
        private SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed = false;
        public const string DeviceDisposedErrorMessage = "This device has already been disposed";
        protected string LogRegion;
        #endregion

        #region Public Abstract Properties
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        public abstract bool IsInitialized { get; }
        #endregion

        #region Public Properties
        public ITracer Tracer { get; set; }
        public ConnectedDeviceDefinitionBase ConnectedDeviceDefinition { get; set; }
        public string DeviceId { get; set; }
        public ILogger Logger { get; set; }
        #endregion

        #region Protected Methods
        protected void Log(string message, Exception ex)
        {
            if (LogRegion == null)
            {
                LogRegion = GetType().Name;
            }

            Logger?.Log(message, $"{ LogRegion} - {1}", ex, ex != null ? LogLevel.Error : LogLevel.Information);
        }

        //protected void Log(string message, Exception ex, [CallerMemberName] string callerMemberName = null)
        //{
        //    if (LogRegion == null)
        //    {
        //        LogRegion = GetType().Name;
        //    }

        //    Logger?.Log(message, $"{ LogRegion} - {callerMemberName}", ex, ex != null ? LogLevel.Error : LogLevel.Information);
        //}
        #endregion

        #region Public Abstract Methods
        public abstract Task<byte[]> ReadAsync();
        public abstract Task WriteAsync(byte[] data);
        #endregion

        #region Public Methods
        public async Task<byte[]> WriteAndReadAsync(byte[] writeBuffer)
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                await WriteAsync(writeBuffer);
                return await ReadAsync();
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        /// <summary> 
        /// Many Hid devices on Windows have a buffer size that is one byte larger than the logical buffer size. For compatibility with other platforms etc. we need to remove the first byte. See DataHasExtraByte
        /// </summary> 
        public static byte[] RemoveFirstByte(byte[] bytes)
        {
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
        }
        #endregion
    }
}
