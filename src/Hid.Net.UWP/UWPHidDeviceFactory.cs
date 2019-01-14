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
        private Dictionary<string, ConnectionInfo> _ConnectionTestedDeviceIds = new Dictionary<string, ConnectionInfo>();

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
        public override async Task<ConnectionInfo> TestConnection(string deviceId)
        {
            try
            {
                await _TestConnectionSemaphore.WaitAsync();

                if (_ConnectionTestedDeviceIds.TryGetValue(deviceId, out var connectionInfo)) return connectionInfo;

                using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
                {
                    var canConnect = hidDevice != null;

                    if (!canConnect) return new ConnectionInfo { CanConnect = false };

                    Logger.Log($"Testing device connection. Id: {deviceId}. Can connect: {canConnect}", null, nameof(UWPHidDeviceFactory));

                    connectionInfo = new ConnectionInfo { CanConnect = canConnect, UsagePage = hidDevice.UsagePage };

                    _ConnectionTestedDeviceIds.Add(deviceId, connectionInfo);

                    return connectionInfo;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("", ex, nameof(UWPHidDeviceFactory));
                return new ConnectionInfo { CanConnect = false };
            }
            finally
            {
                _TestConnectionSemaphore.Release();
            }
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
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