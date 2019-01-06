using Device.Net;
using Device.Net.UWP;
using Device.Net.Windows;

namespace Usb.Net.UWP
{
    public class UWPUsbDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Methods
        public override DeviceType DeviceType => DeviceType.Usb;
        #endregion

        #region Protected Methods
        protected override string GetAqsFilter(uint? vendorId, uint? productId)
        {
            //TODO: This is hard coded for WinUSB devices. Can we use other types of devices? GPS devices for example?
            var interfaceClassGuid = "System.Devices.InterfaceClassGuid:=\"{" + WindowsDeviceConstants.WinUSBGuid + "}\"";


            string vendorPart = null;
            if (vendorId.HasValue) vendorPart = $"AND System.DeviceInterface.WinUsb.UsbVendorId:={vendorId.Value}";

            string productPart = null;
            if (productId.HasValue) productPart = $"AND System.DeviceInterface.WinUsb.UsbProductId:={productId.Value}";

            return $"{interfaceClassGuid} {InterfaceEnabledPart} {vendorPart} {productPart}";
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceType == DeviceType.Hid) return null;
            return new UWPUsbDevice(deviceDefinition.DeviceId);
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
    }
}
