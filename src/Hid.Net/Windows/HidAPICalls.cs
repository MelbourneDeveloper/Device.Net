using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Hid.Net.Windows
{
    public static class HidAPICalls
    {
        #region Private Static Fields
        private static Guid? _HidGuid;
        #endregion

        #region Constants
        private const int HIDP_STATUS_SUCCESS = 0x110000;
        #endregion

        #region API Calls

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetPreparsedData(SafeFileHandle hidDeviceObject, out IntPtr pointerToPreparsedData);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_GetManufacturerString(SafeFileHandle hidDeviceObject, IntPtr pointerToBuffer, uint bufferLength);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_GetProductString(SafeFileHandle hidDeviceObject, IntPtr pointerToBuffer, uint bufferLength);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_GetSerialNumberString(SafeFileHandle hidDeviceObject, IntPtr pointerToBuffer, uint bufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidP_GetCaps(IntPtr pointerToPreparsedData, out HidCollectionCapabilities hidCollectionCapabilities);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetAttributes(SafeFileHandle hidDeviceObject, out HidAttributes attributes);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern void HidD_GetHidGuid(out Guid hidGuid);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_FreePreparsedData(ref IntPtr pointerToPreparsedData);

        private delegate bool GetString(SafeFileHandle hidDeviceObject, IntPtr pointerToBuffer, uint bufferLength);

        #endregion

        #region Private Static Methods
        private static string GetHidString(SafeFileHandle safeFileHandle, GetString getString)
        {
            var pointerToBuffer = Marshal.AllocHGlobal(126);
            var isSuccess = getString(safeFileHandle, pointerToBuffer, 126);
            Marshal.FreeHGlobal(pointerToBuffer);
            WindowsDeviceBase.HandleError(isSuccess, "Could not get Hid string");
            return Marshal.PtrToStringUni(pointerToBuffer);
        }
        #endregion

        #region Public Static Methods
        public static HidAttributes GetHidAttributes(SafeFileHandle safeFileHandle)
        {
            var isSuccess = HidD_GetAttributes(safeFileHandle, out var hidAttributes);
            WindowsDeviceBase.HandleError(isSuccess, $"Could not get Hid Attributes (Call {nameof(HidD_GetAttributes)})");
            return hidAttributes;
        }

        public static HidCollectionCapabilities GetHidCapabilities(SafeFileHandle readSafeFileHandle)
        {
            var isSuccess = HidD_GetPreparsedData(readSafeFileHandle, out var pointerToPreParsedData);
            WindowsDeviceBase.HandleError(isSuccess, "Could not get pre parsed data");

            var result = HidP_GetCaps(pointerToPreParsedData, out var hidCollectionCapabilities);
            if (result != HIDP_STATUS_SUCCESS)
            {
                throw new ApiException($"Could not get Hid capabilities. Return code: {result}");
            }

            isSuccess = HidD_FreePreparsedData(ref pointerToPreParsedData);
            WindowsDeviceBase.HandleError(isSuccess, "Could not release handle for getting Hid capabilities");

            return hidCollectionCapabilities;
        }

        public static string GetManufacturer(SafeFileHandle safeFileHandle)
        {
            return GetHidString(safeFileHandle, HidD_GetManufacturerString);
        }

        public static string GetProduct(SafeFileHandle safeFileHandle)
        {
            return GetHidString(safeFileHandle, HidD_GetProductString);
        }

        public static string GetSerialNumber(SafeFileHandle safeFileHandle)
        {
            return GetHidString(safeFileHandle, HidD_GetSerialNumberString);
        }

        public static Guid GetHidGuid()
        {
            if (_HidGuid.HasValue)
            {
                return _HidGuid.Value;
            }

            HidD_GetHidGuid(out var hidGuid);

            _HidGuid = hidGuid;

            return hidGuid;
        }
        #endregion
    }
}
