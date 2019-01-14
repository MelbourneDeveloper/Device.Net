using Device.Net;
using Device.Net.UWP;
using Device.Net.Windows;
using System.Threading.Tasks;

namespace Usb.Net.UWP
{
    public class UWPUsbDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Usb;
        protected override string VendorFilterName => "System.DeviceInterface.WinUsb.UsbVendorId";
        protected override string ProductFilterName => "System.DeviceInterface.WinUsb.UsbProductId";
        #endregion

        #region Protected Override Methods
        protected override string GetAqsFilter(uint? vendorId, uint? productId)
        {
            //TODO: This is hard coded for WinUSB devices. Can we use other types of devices? GPS devices for example?
            var interfaceClassGuid = "System.Devices.InterfaceClassGuid:=\"{" + WindowsDeviceConstants.WinUSBGuid + "}\"";
            return $"{interfaceClassGuid} {InterfaceEnabledPart} {GetVendorPart(vendorId)} {GetProductPart(productId)}";
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceType == DeviceType.Hid) return null;
            return new UWPUsbDevice(deviceDefinition);
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            foreach (var deviceFactory in DeviceManager.Current.DeviceFactories)
            {
                if (deviceFactory is UWPUsbDeviceFactory) return;
            }

            DeviceManager.Current.DeviceFactories.Add(new UWPUsbDeviceFactory());
        }
        #endregion

        #region Public Overrides
        public override Task<ConnectionInfo> TestConnection(string Id) => Task.FromResult(new ConnectionInfo {CanConnect=true });
        #endregion
    }
}
