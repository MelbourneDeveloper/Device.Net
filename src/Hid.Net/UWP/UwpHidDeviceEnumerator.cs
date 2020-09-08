using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Hid.Net.UWP
{
    public class UwpHidDeviceEnumerator
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly string aqsFilter;
        private readonly SemaphoreSlim _TestConnectionSemaphore = new SemaphoreSlim(1, 1);

        #region Constructor
        protected UwpHidDeviceEnumerator(
            ILoggerFactory loggerFactory,
            ILogger logger,
            string aqf)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger;
            aqsFilter = aqf;
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
        {
            var deviceInformationCollection = aqsFilter != null
                ? await wde.DeviceInformation.FindAllAsync(aqsFilter).AsTask()
                : await wde.DeviceInformation.FindAllAsync().AsTask();

            var deviceInformationList = deviceInformationCollection.ToList();
            var deviceDefinitions = deviceInformationList.Select(d => GetDeviceInformation(d, DeviceType, _logger));

            var deviceDefinitionList = new List<ConnectedDeviceDefinition>();

            foreach (var deviceDef in deviceDefinitions)
            {
                var connectionInformation = await TestConnection(deviceDef.DeviceId);
                if (connectionInformation.CanConnect)
                {
                    deviceDef.UsagePage = connectionInformation.UsagePage;

                    deviceDefinitionList.Add(deviceDef);
                }
            }

            return deviceDefinitionList;
        }

        private async Task<ConnectionInfo> TestConnection(string deviceId)
        {
            IDisposable logScope = null;
            try
            {
                await _TestConnectionSemaphore.WaitAsync();

                logScope = _logger?.BeginScope("DeviceId: {deviceId} Call: {call}", deviceId, nameof(TestConnection));

                if (_ConnectionTestedDeviceIds.TryGetValue(deviceId, out var connectionInfo)) return connectionInfo;

                using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
                {
                    var canConnect = hidDevice != null;

                    if (!canConnect) return new ConnectionInfo { CanConnect = false };

                    _logger?.LogInformation("Testing device connection. Id: {deviceId}. Can connect: {canConnect}", deviceId, canConnect);

                    connectionInfo = new ConnectionInfo { CanConnect = canConnect, UsagePage = hidDevice.UsagePage };

                    _ConnectionTestedDeviceIds.Add(deviceId, connectionInfo);

                    return connectionInfo;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, Messages.ErrorMessageCouldntIntializeDevice);
                return new ConnectionInfo { CanConnect = false };
            }
            finally
            {
                logScope?.Dispose();
                _TestConnectionSemaphore.Release();
            }
        }
        #endregion
    }
}
