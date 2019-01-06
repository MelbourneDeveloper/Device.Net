using Device.Net;
using Device.Net.UWP;

namespace Hid.Net.UWP
{
    public class UWPHidDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        #endregion

        #region Protected Override Methods
        protected override string GetAqsFilter(uint? vendorId, uint? productId)
        {
            return $"System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND System.DeviceInterface.Hid.VendorId:={vendorId} AND System.DeviceInterface.Hid.ProductId:={productId} ";
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceType == DeviceType.Usb) return null;
            return new UWPHidDevice(deviceDefinition.DeviceId);
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            foreach (var deviceFactory in DeviceManager.Current.DeviceFactories)
            {
                if (deviceFactory is UWPHidDeviceFactory) return;
            }

            DeviceManager.Current.DeviceFactories.Add(new UWPHidDeviceFactory());
        }
        #endregion
    }
}