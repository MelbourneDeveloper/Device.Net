using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase
    {
        #region Fields
        protected bool IsReading { get; set; }
        #endregion

        #region Protected Properties
        protected TaskCompletionSource<byte[]> ReadChunkTaskCompletionSource { get; set; }
        protected Collection<byte[]> Chunks { get; } = new Collection<byte[]>();
        protected ILogger Logger { get; }

        #endregion

        #region Public Properties
        public string DeviceId { get; }
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

        #region Constructor
        protected UWPDeviceBase(string deviceId, ILogger logger)
        {
            Logger = logger;
            DeviceId = deviceId;
        }
        #endregion
    }
}
