using Device.Net;
using Device.Net.UWP;
using System;
using System.Threading.Tasks;

namespace Hid.Net.UWP
{
    public class UWPHidDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        protected override string VendorFilterName => "System.DeviceInterface.Hid.VendorId";
        protected override string ProductFilterName => "System.DeviceInterface.Hid.ProductId";
        #endregion

        #region Protected Override Methods
        protected override string GetAqsFilter(uint? vendorId, uint? productId)
        {
            return $"{InterfaceEnabledPart} {GetVendorPart(vendorId)} {GetProductPart(productId)}";
        }
        #endregion

        #region Public Override Methods
        //TODO: This is pretty inefficient but, not a lot can be done as far as I can tell...
        public async override Task<bool> TestConnection(string deviceId)
        {
            using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
            {
                var canConnect = hidDevice != null;

                Logger.Log($"Testing device connection. Id: {deviceId}. Can connect: {canConnect}", null, nameof(UWPHidDeviceFactory));

                return canConnect;
            }
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