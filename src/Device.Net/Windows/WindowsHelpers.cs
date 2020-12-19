using Device.Net.Exceptions;
using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    public static class WindowsHelpers
    {
        public static int HandleError(bool isSuccess, string message, bool throwException = true)
        {
            if (isSuccess) return 0;
            var errorCode = Marshal.GetLastWin32Error();

            return errorCode == 0 ? 0 : throwException ? throw new ApiException($"{message}. Error code: {errorCode}") : errorCode;
        }
    }
}
