using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    public static class WindowsHelpers
    {
        public static int HandleError(bool isSuccess, string message, ILogger logger, bool throwException = true)
        {
            logger ??= NullLogger.Instance;

            if (isSuccess) return 0;

            var errorCode = Marshal.GetLastWin32Error();

            if (errorCode == 0)
            {
                return 0;
            }

            if (throwException)
            {
                var apiException = new ApiException($"{message}. Error code: {errorCode}");
                logger.LogError(new EventId(errorCode), apiException, "Windows error", errorCode);
                throw apiException;
            }

            return errorCode;
        }
    }
}
