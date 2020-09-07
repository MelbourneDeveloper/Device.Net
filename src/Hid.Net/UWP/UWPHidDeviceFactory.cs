using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net.UWP
{
    public sealed class UWPHidDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory, IDisposable
    {
        #region Fields
        private readonly ILoggerFactory _loggerFactory;
        private readonly SemaphoreSlim _TestConnectionSemaphore = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, ConnectionInfo> _ConnectionTestedDeviceIds = new Dictionary<string, ConnectionInfo>();
        private bool disposed;
        #endregion

        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        protected override string VendorFilterName => "System.DeviceInterface.Hid.VendorId";
        protected override string ProductFilterName => "System.DeviceInterface.Hid.ProductId";
        #endregion

        #region Protected Override Methods
        protected override string GetAqsFilter(uint? vendorId, uint? productId) => $"{InterfaceEnabledPart} {GetVendorPart(vendorId)} {GetProductPart(productId)}";
        #endregion

        #region Public Override Methods
        public override async Task<ConnectionInfo> TestConnection(string deviceId)
        {
            IDisposable logScope = null;
            try
            {
                await _TestConnectionSemaphore.WaitAsync();

                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", deviceId, nameof(TestConnection));

                if (_ConnectionTestedDeviceIds.TryGetValue(deviceId, out var connectionInfo)) return connectionInfo;

                using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
                {
                    var canConnect = hidDevice != null;

                    if (!canConnect) return new ConnectionInfo { CanConnect = false };

                    Logger?.LogInformation("Testing device connection. Id: {deviceId}. Can connect: {canConnect}", deviceId, canConnect);

                    connectionInfo = new ConnectionInfo { CanConnect = canConnect, UsagePage = hidDevice.UsagePage };

                    _ConnectionTestedDeviceIds.Add(deviceId, connectionInfo);

                    return connectionInfo;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageCouldntIntializeDevice);
                return new ConnectionInfo { CanConnect = false };
            }
            finally
            {
                logScope?.Dispose();
                _TestConnectionSemaphore.Release();
            }
        }
        #endregion

        #region Constructor
        public UWPHidDeviceFactory(ILoggerFactory loggerFactory) : base(loggerFactory, loggerFactory.CreateLogger<UWPHidDeviceFactory>())
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return deviceDefinition == null
                ? throw new ArgumentNullException(nameof(deviceDefinition))
                : deviceDefinition.DeviceType == DeviceType.Usb ? null : new UWPHidDevice(deviceDefinition.DeviceId, _loggerFactory);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            _TestConnectionSemaphore.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Finalizer
        ~UWPHidDeviceFactory()
        {
            Dispose();
        }
        #endregion
    }
}