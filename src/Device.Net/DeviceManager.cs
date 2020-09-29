using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net
{
    public class DeviceManager : IDeviceManager
    {
        //TODO: Put logging in here

        #region Fields
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        #endregion

        #region Public Properties
        public const string ObsoleteMessage = "This method will soon be removed. Create an instance of DeviceManager and register factories there";
        public IReadOnlyCollection<IDeviceFactory> DeviceFactories { get; }
        public bool IsInitialized => DeviceFactories.Count > 0;
        #endregion

        #region Constructor
        public DeviceManager(
            IReadOnlyCollection<IDeviceFactory> deviceFactories,
            ILoggerFactory loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? new NullLoggerFactory();
            _logger = _loggerFactory.CreateLogger<DeviceManager>();
            DeviceFactories = deviceFactories;
        }
        #endregion

        #region Public Methods
        public async Task<IReadOnlyCollection<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
        {
            if (DeviceFactories.Count == 0) throw new DeviceFactoriesNotRegisteredException();

            var retVal = new List<ConnectedDeviceDefinition>();

            foreach (var deviceFactory in DeviceFactories)
            {
                try
                {
                    //TODO: Do this in parallel?
                    var factoryResults = await deviceFactory.GetConnectedDeviceDefinitionsAsync();
                    retVal.AddRange(factoryResults);

                    _logger.LogDebug("Called " + nameof(GetConnectedDeviceDefinitionsAsync) + " on " + deviceFactory.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling " + nameof(GetConnectedDeviceDefinitionsAsync));
                }
            }

            return retVal;
        }

        //TODO: Duplicate code here...
        public Task<IDevice> GetDevice(ConnectedDeviceDefinition connectedDeviceDefinition)
        {
            if (connectedDeviceDefinition == null) throw new ArgumentNullException(nameof(connectedDeviceDefinition));

            foreach (var deviceFactory in DeviceFactories.Where(deviceFactory => !connectedDeviceDefinition.DeviceType.HasValue || (deviceFactory.DeviceType == connectedDeviceDefinition.DeviceType)))
            {
                return deviceFactory.GetDevice(connectedDeviceDefinition);
            }

            throw new DeviceException(Messages.ErrorMessageCouldntGetDevice);
        }
        #endregion

        #region Public Static Methods
        public static bool IsDefinitionMatch(FilterDeviceDefinition filterDevice, ConnectedDeviceDefinition actualDevice, DeviceType deviceType)
        {
            if (actualDevice == null) throw new ArgumentNullException(nameof(actualDevice));

            if (filterDevice == null) return true;

            var vendorIdPasses = !filterDevice.VendorId.HasValue || filterDevice.VendorId == actualDevice.VendorId;
            var productIdPasses = !filterDevice.ProductId.HasValue || filterDevice.ProductId == actualDevice.ProductId;
            var deviceTypePasses = !actualDevice.DeviceType.HasValue || actualDevice.DeviceType == deviceType;
            var usagePagePasses = !filterDevice.UsagePage.HasValue || filterDevice.UsagePage == actualDevice.UsagePage;

            var returnValue =
                vendorIdPasses &&
                productIdPasses &&
                deviceTypePasses &&
                usagePagePasses;

            return returnValue;
        }
        #endregion
    }
}
