using Device.Net;
using Device.Net.UWP;

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
            return "System.Devices.InterfaceClassGuid:=\"{DEE824EF-729B-4A0E-9C14-B7117D33A817}\" AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND " + $" System.DeviceInterface.WinUsb.UsbVendorId:={vendorId.Value} AND System.DeviceInterface.WinUsb.UsbProductId:={productId.Value}";
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
