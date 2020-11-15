using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{

    public class DeviceManager : IDeviceFactory
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
            DeviceFactories = deviceFactories ?? throw new ArgumentNullException(nameof(deviceFactories));

            if (deviceFactories.Count == 0)
            {
                throw new InvalidOperationException("You must specify at least one Device Factory");
            }

        }
        #endregion

        #region Public Methods
        public async Task<bool> SupportsDevice(ConnectedDeviceDefinition deviceDefinition) => (await DeviceFactories.FirstOrDefaultAsync(async d => await d.SupportsDevice(deviceDefinition))) != null;

        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
        {
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

        public async Task<IDevice> GetDevice(ConnectedDeviceDefinition connectedDeviceDefinition)
             => connectedDeviceDefinition == null ? throw new ArgumentNullException(nameof(connectedDeviceDefinition)) :
            await ((await DeviceFactories.FirstOrDefaultAsync(f => f.SupportsDevice(connectedDeviceDefinition)))
            ?? throw new DeviceException(Messages.ErrorMessageCouldntGetDevice))
            .GetDevice(connectedDeviceDefinition);

        #endregion
    }
}
