using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using static Hid.Net.Windows.HidAPICalls;

namespace Hid.Net.Windows
{
    public class WindowsHidDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        #endregion

        #region Protected Override Methods
        protected override ConnectedDeviceDefinition GetDeviceDefinition(string deviceId)
        {
            using (var safeFileHandle = APICalls.CreateFile(deviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero))
            {
                return GetDeviceDefinition(deviceId, safeFileHandle);
            }
        }

        protected override Guid GetClassGuid()
        {
            return GetHidGuid();
        }

        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsHidDevice(deviceDefinition.DeviceId) { Logger = Logger };
        }
        #endregion

        #region Private Static Methods
        public static ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, SafeFileHandle safeFileHandle)
        {
            try
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
            catch (Exception)
            {
                //TODO: Logging...

                //This probably needs a big refactor. Because this is a static method, it either needs to have the logger passed in, or it should be a non-static method

                return null;
            }
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            Register(null);
        }

        public static void Register(ILogger logger)
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsHidDeviceFactory() { Logger = logger });
        }
        #endregion
    }
}
