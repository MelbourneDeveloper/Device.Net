using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
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
        protected WindowsDeviceBase(string deviceId, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
        }
        #endregion

        #region Public Methods
        public abstract Task InitializeAsync();
        #endregion

        #region Public Static Methods
        public static void HandleError(bool isSuccess, string message)
        {
            if (isSuccess) return;
            var errorCode = Marshal.GetLastWin32Error();

            //TODO: Loggin
            if (errorCode == 0) return;

            throw new ApiException($"{message}. Error code: {errorCode}");
        }
        #endregion
    }
}