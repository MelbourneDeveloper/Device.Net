using Device.Net;
using Microsoft.Win32.SafeHandles;
using System;

namespace Hid.Net.Windows
{
    public class WindowsHidApiService : IHidService
    {
        #region Public Properties
        public ILogger Logger { get; }
        #endregion

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

        public WindowsHidApiService(ILogger logger)
        {
            Logger = logger;
        }

        public HidAttributes GetHidAttributes(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetHidAttributes(safeFileHandle);
        }

        public HidCollectionCapabilities GetHidCapabilities(SafeFileHandle readSafeFileHandle)
        {
            return HidAPICalls.GetHidCapabilities(readSafeFileHandle);
        }

        public string GetManufacturer(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetManufacturer(safeFileHandle, Logger);
        }

        public string GetProduct(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetProduct(safeFileHandle, Logger);
        }

        public string GetSerialNumber(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetSerialNumber(safeFileHandle, Logger);
        }

        public Guid GetHidGuid()
        {
            return HidAPICalls.GetHidGuid();
        }
    }
}
