using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hid.Net.Windows
{
    public class WindowsHidApiService : ApiService, IHidApiService
    {
        #region Private Static Fields
        private static Guid? _HidGuid;
        #endregion

        #region Constants
        private const int HIDP_STATUS_SUCCESS = 0x110000;
        #endregion

        #region Constructor
        public WindowsHidApiService(ILogger logger) : base(logger)
        {
        }
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

        #region Implementation
        public ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, SafeFileHandle safeFileHandle)
        {
            var hidAttributes = GetHidAttributes(safeFileHandle);
            var hidCollectionCapabilities = GetHidCapabilities(safeFileHandle);

            var manufacturer = GetManufacturer(safeFileHandle);
            var serialNumber = GetSerialNumber(safeFileHandle);
            var product = GetProduct(safeFileHandle);

            return new ConnectedDeviceDefinition(deviceId)
            {
                WriteBufferSize = hidCollectionCapabilities.OutputReportByteLength,
                ReadBufferSize = hidCollectionCapabilities.InputReportByteLength,
                Manufacturer = manufacturer,
                ProductName = product,
                ProductId = (ushort)hidAttributes.ProductId,
                SerialNumber = serialNumber,
                Usage = hidCollectionCapabilities.Usage,
                UsagePage = hidCollectionCapabilities.UsagePage,
                VendorId = (ushort)hidAttributes.VendorId,
                VersionNumber = (ushort)hidAttributes.VersionNumber,
                DeviceType = DeviceType.Hid
            };
        }

        public string GetManufacturer(SafeFileHandle safeFileHandle) => GetHidString(safeFileHandle, HidD_GetManufacturerString, Logger);

        public string GetProduct(SafeFileHandle safeFileHandle) => GetHidString(safeFileHandle, HidD_GetProductString, Logger);

        public string GetSerialNumber(SafeFileHandle safeFileHandle) => GetHidString(safeFileHandle, HidD_GetSerialNumberString, Logger);

        public HidAttributes GetHidAttributes(SafeFileHandle safeFileHandle)
        {
            var isSuccess = HidD_GetAttributes(safeFileHandle, out var hidAttributes);
            WindowsDeviceBase.HandleError(isSuccess, $"Could not get Hid Attributes (Call {nameof(HidD_GetAttributes)})");
            return hidAttributes;
        }

        public HidCollectionCapabilities GetHidCapabilities(SafeFileHandle readSafeFileHandle)
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

        public Guid GetHidGuid()
        {
            if (_HidGuid.HasValue)
            {
                return _HidGuid.Value;
            }

            HidD_GetHidGuid(out var hidGuid);

            _HidGuid = hidGuid;

            return hidGuid;
        }

        public Stream OpenRead(SafeFileHandle readSafeFileHandle, ushort readBufferSize) => new FileStream(readSafeFileHandle, FileAccess.Read, readBufferSize, false);

        public Stream OpenWrite(SafeFileHandle writeSafeFileHandle, ushort writeBufferSize) => new FileStream(writeSafeFileHandle, FileAccess.ReadWrite, writeBufferSize, false);
        #endregion

        #region Private Methods
        private static string GetHidString(SafeFileHandle safeFileHandle, GetString getString, ILogger logger, [CallerMemberName] string callMemberName = null)
        {
            try
            {
                var pointerToBuffer = Marshal.AllocHGlobal(126);
                var isSuccess = getString(safeFileHandle, pointerToBuffer, 126);
                if (!isSuccess)
                {
                    logger?.Log($"Could not get Hid string. Caller: {callMemberName}", nameof(WindowsHidApiService), null, LogLevel.Warning);
                }
                var text = Marshal.PtrToStringAuto(pointerToBuffer);
                Marshal.FreeHGlobal(pointerToBuffer);
                return text;
            }
            catch (Exception ex)
            {
                logger?.Log($"Could not get Hid string. Message: {ex.Message}", nameof(WindowsHidApiService), ex, LogLevel.Error);
                return null;
            }
            finally
            {
                //TODO: Shouldn't this pointer be released?
                //Marshal.Release(pointerToBuffer);
            }
        }
        #endregion
    }
}
