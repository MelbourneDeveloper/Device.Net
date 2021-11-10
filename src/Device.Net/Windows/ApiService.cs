using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1021 // Avoid out parameters
#pragma warning disable CA5392 // Use DefaultDllImportSearchPaths attribute for P/Invokes
#pragma warning disable CA1060 // Move pinvokes to native methods class

namespace Device.Net.Windows
{
    internal class ApiService : IApiService
    {
        #region Fields
        private const uint FILE_FLAG_OVERLAPPED = 0x40000000;

        private readonly uint fileFlag;

        protected ILogger Logger { get; }
        #endregion

        #region Constructor
        public ApiService(ILogger logger = null)
        {
            Logger = logger ?? NullLogger.Instance;
            fileFlag = FILE_FLAG_OVERLAPPED;
            var runtime = RuntimeInformation.FrameworkDescription;
            if (runtime.StartsWith(".NET Framework", StringComparison.InvariantCultureIgnoreCase))
            {
                fileFlag = 0;
            }
            else if (Regex.Match(runtime, @"^\.NET (\d+)") is { Success: true } m)
            {
                var majorVersion = m.Groups[1].Value;
                if (majorVersion != "5")
                {
                    var isSwitchConfigured = AppContext.TryGetSwitch("System.IO.UseNet5CompatFileStream", out var isSwitchEnabled);
                    if (!isSwitchConfigured || !isSwitchEnabled)
                    {
                        fileFlag = 0;
                    }
                }
            }
        }
        #endregion

        #region Implementation
        public SafeFileHandle CreateWriteConnection(string deviceId) => CreateConnection(deviceId, FileAccessRights.GenericRead | FileAccessRights.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, APICalls.OpenExisting);

        public SafeFileHandle CreateReadConnection(string deviceId, FileAccessRights desiredAccess) => CreateConnection(deviceId, desiredAccess, APICalls.FileShareRead | APICalls.FileShareWrite, APICalls.OpenExisting);

        public bool AGetCommState(SafeFileHandle hFile, ref Dcb lpDCB) => GetCommState(hFile, ref lpDCB);
        public bool APurgeComm(SafeFileHandle hFile, int dwFlags) => PurgeComm(hFile, dwFlags);
        public bool ASetCommTimeouts(SafeFileHandle hFile, ref CommTimeouts lpCommTimeouts) => SetCommTimeouts(hFile, ref lpCommTimeouts);
        public bool AWriteFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, int lpOverlapped) => WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, out lpNumberOfBytesWritten, lpOverlapped);
        public bool AReadFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out uint lpNumberOfBytesRead, int lpOverlapped) => ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, out lpNumberOfBytesRead, lpOverlapped);
        public bool ASetCommState(SafeFileHandle hFile, [In] ref Dcb lpDCB) => SetCommState(hFile, ref lpDCB);
        #endregion

        #region Private Methods
        private SafeFileHandle CreateConnection(string deviceId, FileAccessRights desiredAccess, uint shareMode, uint creationDisposition)
        {
            Logger.LogInformation("Calling {call} Area: {area} for DeviceId: {deviceId}. Desired Access: {desiredAccess}. Share mode: {shareMode}. Creation Disposition: {creationDisposition}", nameof(APICalls.CreateFile), nameof(ApiService), deviceId, desiredAccess, shareMode, creationDisposition);
            return APICalls.CreateFile(deviceId, desiredAccess, shareMode, IntPtr.Zero, creationDisposition, fileFlag, IntPtr.Zero);
        }
        #endregion

        #region DLL Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool PurgeComm(SafeFileHandle hFile, int dwFlags);
        [DllImport("kernel32.dll")]
        private static extern bool GetCommState(SafeFileHandle hFile, ref Dcb lpDCB);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommTimeouts(SafeFileHandle hFile, ref CommTimeouts lpCommTimeouts);
        [DllImport("kernel32.dll")]
        private static extern bool SetCommState(SafeFileHandle hFile, [In] ref Dcb lpDCB);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, int lpOverlapped);
        [DllImport("kernel32.dll")]
        private static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out uint lpNumberOfBytesRead, int lpOverlapped);
        #endregion
    }
}
