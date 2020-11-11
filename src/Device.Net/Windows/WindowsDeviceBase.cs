using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    /// <summary>
    /// This class remains untested
    /// </summary>
    public abstract class WindowsDeviceBase : DeviceBase
    {
        #region Protected Properties
        protected virtual string LogSection => nameof(WindowsDeviceBase);
        #endregion

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

        #region Public Static Methods
        public static int HandleError(bool isSuccess, string message, bool throwException = true)
        {
            if (isSuccess) return 0;
            var errorCode = Marshal.GetLastWin32Error();

            //TODO: Loggin
            return errorCode == 0 ? 0 : throwException ? throw new ApiException($"{message}. Error code: {errorCode}") : errorCode;
        }
        #endregion
    }
}