using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    [SuppressMessage("NDepend", "ND2400:PInvokesShouldBeStaticAndNotBePubliclyVisible", Justification = "...")]
    public static class APICalls
    {
        #region Constants
        public const int DigcfDeviceinterface = 16;
        public const int DigcfPresent = 2;
        public const uint FileShareRead = 1;
        public const uint FileShareWrite = 2;

        public const uint OpenExisting = 3;
        public const int FileAttributeNormal = 128;
        public const int FileFlagOverlapped = 1073741824;

        public const int ERROR_NO_MORE_ITEMS = 259;

        public const int PURGE_TXCLEAR = 0x0004;
        public const int PURGE_RXCLEAR = 0x0008;
        #endregion

        #region Methods

        #region Kernel32
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFileHandle CreateFile(string lpFileName, FileAccessRights dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        // Used to read bytes from the serial connection. 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out int lpNumberOfBytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, int lpOverlapped);
        #endregion

        #region SetupAPI
        [SuppressMessage("NDepend", "ND2400:PInvokesShouldBeStaticAndNotBePubliclyVisible", Justification = "...")]
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/setupapi/nf-setupapi-setupdienumdeviceinterfaces
        /// </summary>
        [SuppressMessage("NDepend", "ND2400:PInvokesShouldBeStaticAndNotBePubliclyVisible", Justification = "...")]
        [DllImport(@"setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SpDeviceInterfaceData deviceInterfaceData);

        [SuppressMessage("NDepend", "ND2400:PInvokesShouldBeStaticAndNotBePubliclyVisible", Justification = "...")]
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, IntPtr enumerator, IntPtr hwndParent, uint flags);

        [SuppressMessage("NDepend", "ND2400:PInvokesShouldBeStaticAndNotBePubliclyVisible", Justification = "...")]
        [DllImport(@"setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SpDeviceInterfaceData deviceInterfaceData, ref SpDeviceInterfaceDetailData deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, ref SpDeviceInfoData deviceInfoData);
        #endregion

        #endregion
    }
}
