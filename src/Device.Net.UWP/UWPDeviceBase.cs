using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase : DeviceBase
    {
        #region Fields
        protected bool IsReading { get; set; }
        #endregion

        #region Protected Properties
        protected TaskCompletionSource<byte[]> ReadChunkTaskCompletionSource { get; set; }
        protected Collection<byte[]> Chunks { get; } = new Collection<byte[]>();
        #endregion

        #region Protected Methods
        protected void HandleDataReceived(byte[] bytes)
        {
            if (!IsReading)
            {
                lock (Chunks)
                {
                    Chunks.Add(bytes);
                }
            }
            else
            {
                IsReading = false;
                ReadChunkTaskCompletionSource.SetResult(bytes);
            }
        }
        #endregion

        #region Public Abstract Methods
        public abstract Task InitializeAsync();
        #endregion
    }
}
