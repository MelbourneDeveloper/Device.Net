using Device.Net;
using Device.Net.UWP;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net.UWP
{
    public class UWPHidDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory
    {
        private SemaphoreSlim _TestConnectionSemaphore = new SemaphoreSlim(1, 1);
        private Dictionary<string, bool> _ConnectionTestedDeviceIds = new Dictionary<string, bool>();

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

        protected override async Task<ushort?> GetUsagePageAsync(string deviceId)
        {
            using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
            {
                if (hidDevice != null) return hidDevice.UsagePage;
            }

            throw new Exception("Could not get UsagePage");
        }
        #endregion

        #region Public Override Methods
        public override async Task<bool> TestConnection(string deviceId)
        {
            try
            {
                await _TestConnectionSemaphore.WaitAsync();

                if (_ConnectionTestedDeviceIds.TryGetValue(deviceId, out var canConnect)) return canConnect;

                using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
                {
                    canConnect = hidDevice != null;

                    Logger.Log($"Testing device connection. Id: {deviceId}. Can connect: {canConnect}", null, nameof(UWPHidDeviceFactory));

                    _ConnectionTestedDeviceIds.Add(deviceId, canConnect);

                    return canConnect;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("", ex, nameof(UWPHidDeviceFactory));
                return false;
            }
            finally
            {
                _TestConnectionSemaphore.Release();
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