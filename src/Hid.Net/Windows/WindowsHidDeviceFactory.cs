using Device.Net;
using Device.Net.Windows;
using System;
using static Hid.Net.Windows.HidAPICalls;

namespace Hid.Net.Windows
{
    public class WindowsHidDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        public override Guid ClassGuid { get; set; } = WindowsDeviceConstants.GUID_DEVINTERFACE_HID;
        #endregion

        #region Public Methods
        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsHidDevice(deviceDefinition);
        }
        #endregion

        #region Private Static Methods
        protected override DeviceDefinition GetDeviceDefinition(string deviceId)
        {
            using (var safeFileHandle = APICalls.CreateFile(deviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero))
            {
                var hidAttributes = GetHidAttributes(safeFileHandle);
                var hidCollectionCapabilities = GetHidCapabilities(safeFileHandle);
                var manufacturer = GetManufacturer(safeFileHandle);
                var serialNumber = GetSerialNumber(safeFileHandle);
                var product = GetProduct(safeFileHandle);

                var deviceInformation = new WindowsHidDeviceDefinition
                {
                    DeviceId = deviceId,
                    //TODO Is this the right way around?
                    WriteBufferSize = hidCollectionCapabilities.InputReportByteLength,
                    ReadBufferSize = hidCollectionCapabilities.OutputReportByteLength,
                    Manufacturer = manufacturer,
                    Product = product,
                    ProductId = (ushort)hidAttributes.ProductId,
                    SerialNumber = serialNumber,
                    Usage = hidCollectionCapabilities.Usage,
                    UsagePage = hidCollectionCapabilities.UsagePage,
                    VendorId = (ushort)hidAttributes.VendorId,
                    VersionNumber = (ushort)hidAttributes.VersionNumber,
                    DeviceType = DeviceType.Hid
                };

                return deviceInformation;
            }
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsHidDeviceFactory());
        }
        #endregion
    }
}
