using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    /// <summary>
    /// This class remains untested
    /// </summary>
    public abstract class WindowsDeviceBase : DeviceBase
    {
        #region Constructor
        protected WindowsDeviceBase(
            string deviceId,
            ILoggerFactory loggerFactory,
            ILogger logger) : base(
                deviceId,
                loggerFactory,
                logger)
        {
        }
        #endregion

        #region Public Methods
        public abstract Task InitializeAsync(CancellationToken cancellationToken = default);
        #endregion
    }
}