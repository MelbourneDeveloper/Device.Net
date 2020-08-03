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
        #endregion

        #region Public Properties
        public ILogger Logger { get; }
        public ITracer Tracer { get; }
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
        protected UWPDeviceBase(string deviceId, ILogger logger, ITracer tracer)
        {
            Logger = logger;
            Tracer = tracer;
            DeviceId = deviceId;
        }
        #endregion
    }
}
