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

        #region Public Properties
        public ITracer Tracer { get; set; }
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
            catch
            {
                throw;
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }
        #endregion
    }
}
