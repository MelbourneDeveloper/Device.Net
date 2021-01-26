
// ReSharper disable IdentifierTypo

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1021 // Avoid out parameters
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable CA5392 // Use DefaultDllImportSearchPaths attribute for P/Invokes
#pragma warning disable CA1045 // Do not pass types by reference
#pragma warning disable CA1060 // Move pinvokes to native methods class

namespace Device.Net.Windows
{
    internal static class APICalls
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
        #endregion

        #region SetupAPI
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/setupapi/nf-setupapi-setupdienumdeviceinterfaces
        /// </summary>
        [DllImport(@"setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SpDeviceInterfaceData deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, IntPtr enumerator, IntPtr hwndParent, uint flags);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SpDeviceInterfaceData deviceInterfaceData, ref SpDeviceInterfaceDetailData deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, ref SpDeviceInfoData deviceInfoData);
        #endregion

        #endregion
    }
}