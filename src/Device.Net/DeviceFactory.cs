using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    public delegate Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(CancellationToken cancellationToken = default);
    public delegate ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, Guid classGuid);
    public delegate Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition deviceId, CancellationToken cancellationToken = default);

    public sealed class DeviceFactory : IDeviceFactory
    {
        #region Fields
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger _logger;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly ILoggerFactory _loggerFactory;
        private readonly GetConnectedDeviceDefinitionsAsync _getConnectedDevicesAsync;
        private readonly GetDeviceAsync _getDevice;
        private readonly Func<ConnectedDeviceDefinition, CancellationToken, Task<bool>> _supportsDevice;
        #endregion

        #region Constructor
        /// <param name="loggerFactory">The factory for creating new loggers for each device</param>
        /// <param name="logger">The logger that this base class will use. The generic type should come from the inheriting class</param>
        public DeviceFactory(

            ILoggerFactory loggerFactory,
            GetConnectedDeviceDefinitionsAsync getConnectedDevicesAsync,
            GetDeviceAsync getDevice,
            Func<ConnectedDeviceDefinition, CancellationToken, Task<bool>> supportsDevice
            )
        {
            _getConnectedDevicesAsync = getConnectedDevicesAsync ?? throw new ArgumentNullException(nameof(getConnectedDevicesAsync));
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = _loggerFactory.CreateLogger<DeviceFactory>();
            _getDevice = getDevice;
            _supportsDevice = supportsDevice ?? throw new ArgumentNullException(nameof(supportsDevice));
        }
        #endregion

        #region Public Methods
        public Task<bool> SupportsDeviceAsync(ConnectedDeviceDefinition deviceDefinition, CancellationToken cancellationToken = default) => _supportsDevice(deviceDefinition, cancellationToken);
        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(CancellationToken cancellationToken = default) => _getConnectedDevicesAsync(cancellationToken);
        public Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition deviceDefinition, CancellationToken cancellationToken = default) => deviceDefinition == null ? throw new ArgumentNullException(nameof(deviceDefinition)) : _getDevice(deviceDefinition, cancellationToken);
        #endregion
    }
}
