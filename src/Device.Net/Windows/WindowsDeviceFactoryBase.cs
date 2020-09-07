using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    public delegate Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDevicesAsync(DeviceType deviceType);

    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public abstract class WindowsDeviceFactoryBase
    {
        private readonly GetConnectedDevicesAsync _getConnectedDevicesAsync;

        #region Protected Properties
        protected ILogger Logger { get; }
        protected ILoggerFactory LoggerFactory { get; }
        #endregion

        public IReadOnlyCollection<FilterDeviceDefinition> FilterDeviceDefinition { get; }

        #region Public Abstract Properties
        public abstract DeviceType DeviceType { get; }
        #endregion

        #region Protected Abstract Methods
        protected abstract ConnectedDeviceDefinition GetDeviceDefinition(string deviceId);
        protected abstract Guid GetClassGuid();
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory">The factory for creating new loggers for each device</param>
        /// <param name="logger">The logger that this base class will use. The generic type should come from the inheriting class</param>
        /// 
        protected WindowsDeviceFactoryBase(
            ILoggerFactory loggerFactory,
            ILogger logger,
            GetConnectedDevicesAsync getConnectedDevicesAsync)
        {
            _getConnectedDevicesAsync = getConnectedDevicesAsync ?? throw new ArgumentNullException(nameof(getConnectedDevicesAsync));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            Logger = logger;
        }
        #endregion

        #region Public Methods
        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync() => _getConnectedDevicesAsync(DeviceType);
        #endregion
    }
}
