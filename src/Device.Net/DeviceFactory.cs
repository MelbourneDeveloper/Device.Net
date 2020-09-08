using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    public delegate Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
    public delegate Guid GetClassGuid();
    public delegate ConnectedDeviceDefinition GetDeviceDefinition(string deviceId);
    public delegate IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition);

    public sealed class DeviceFactory : IDeviceFactory
    {
        #region Protected Properties
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger _logger;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly ILoggerFactory _loggerFactory;
        private readonly GetConnectedDeviceDefinitionsAsync _getConnectedDevicesAsync;
        private readonly GetDevice _getDevice;
        #endregion

        #region Public  Properties
        public DeviceType DeviceType { get; }
        #endregion

        #region Constructor
        /// <param name="loggerFactory">The factory for creating new loggers for each device</param>
        /// <param name="logger">The logger that this base class will use. The generic type should come from the inheriting class</param>
        public DeviceFactory(

            ILoggerFactory loggerFactory,
            GetConnectedDeviceDefinitionsAsync getConnectedDevicesAsync,
            GetDevice getDevice
            )
        {
            _getConnectedDevicesAsync = getConnectedDevicesAsync ?? throw new ArgumentNullException(nameof(getConnectedDevicesAsync));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<DeviceFactory>();
            _getDevice = getDevice;
        }
        #endregion

        #region Public Methods
        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync() => _getConnectedDevicesAsync();

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition) => _getDevice(deviceDefinition);
        #endregion
    }
}
