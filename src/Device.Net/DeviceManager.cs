using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{

    internal class AggregateDeviceFactory : IDeviceFactory
    {
        //TODO: Put logging in here

        #region Fields
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        #endregion

        #region Public Properties
        public const string ObsoleteMessage = "This method will soon be removed. Create an instance of DeviceManager and register factories there";
        public IReadOnlyCollection<IDeviceFactory> DeviceFactories { get; }
        #endregion

        #region Constructor
        public AggregateDeviceFactory(
            IReadOnlyCollection<IDeviceFactory> deviceFactories,
            ILoggerFactory loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? new NullLoggerFactory();
            _logger = _loggerFactory.CreateLogger<AggregateDeviceFactory>();
            DeviceFactories = deviceFactories ?? throw new ArgumentNullException(nameof(deviceFactories));

            if (deviceFactories.Count == 0)
            {
                throw new InvalidOperationException("You must specify at least one Device Factory");
            }

        }
        #endregion

        #region Public Methods
        public async Task<bool> SupportsDeviceAsync(ConnectedDeviceDefinition connectedDeviceDefinition, CancellationToken cancellationToken = default) => await DeviceFactories.FirstOrDefaultAsync(async d => await d.SupportsDeviceAsync(connectedDeviceDefinition, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false) != null;

        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(CancellationToken cancellationToken = default)
        {
            var retVal = new List<ConnectedDeviceDefinition>();

            foreach (var deviceFactory in DeviceFactories)
            {
                try
                {
                    //TODO: Do this in parallel?
                    var factoryResults = await deviceFactory.GetConnectedDeviceDefinitionsAsync(cancellationToken).ConfigureAwait(false);
                    retVal.AddRange(factoryResults);

                    _logger.LogDebug("Called " + nameof(GetConnectedDeviceDefinitionsAsync) + " on " + deviceFactory.GetType().Name);
                }
                catch (Exception ex)
                {
                    //TODO: We probably want to remove this. If a factory crashes, we probably don't want to swallow the error

                    _logger.LogError(ex, "Error calling " + nameof(GetConnectedDeviceDefinitionsAsync));
                }
            }

            return retVal;
        }

        public async Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition connectedDeviceDefinition, CancellationToken cancellationToken = default)
             => connectedDeviceDefinition == null ? throw new ArgumentNullException(nameof(connectedDeviceDefinition)) :
            await (await DeviceFactories.FirstOrDefaultAsync(f => f.SupportsDeviceAsync(connectedDeviceDefinition, cancellationToken), cancellationToken).ConfigureAwait(false)
            ?? throw new DeviceException(Messages.ErrorMessageCouldntGetDevice))
            .GetDeviceAsync(connectedDeviceDefinition, cancellationToken).ConfigureAwait(false);

        #endregion
    }
}
