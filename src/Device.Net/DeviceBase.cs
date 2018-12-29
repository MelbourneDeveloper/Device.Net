using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    public abstract class DeviceBase
    {
        #region Fields
        protected SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        #endregion

        #region Events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        #endregion

        #region Public Abstract Properties
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        #endregion

        #region Public Properties
        public ITracer Tracer { get; set; }
        public DeviceDefinition DeviceDefinition { get; protected set; }
        #endregion

        #region Protected Methods
        protected void RaiseConnected()
        {
            Connected?.Invoke(this, new EventArgs());
        }

        protected void RaiseDisconnected()
        {
            Disconnected?.Invoke(this, new EventArgs());
        }
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
        #endregion
    }
}
