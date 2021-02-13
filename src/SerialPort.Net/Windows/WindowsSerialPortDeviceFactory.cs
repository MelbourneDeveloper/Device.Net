using Device.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Threading;
#if NETSTANDARD
using System.Runtime.InteropServices;
using Device.Net.Exceptions;
#endif

namespace SerialPort.Net.Windows
{
    public class WindowsSerialPortDeviceFactory : IDeviceFactory
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        #endregion

        #region Public Properties
        public IEnumerable<DeviceType> SupportedDeviceTypes { get; } = new ReadOnlyCollection<DeviceType>(new List<DeviceType> { DeviceType.SerialPort });
        #endregion

        #region Constructor
        public WindowsSerialPortDeviceFactory(ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

            //Note this loggerFactory may get shared with other factories of this type
            _logger = _loggerFactory.CreateLogger<WindowsSerialPortDeviceFactory>();
        }
        #endregion

        #region Public Methods


        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(CancellationToken cancellationToken = default)
        {
#if NETSTANDARD
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new ValidationException(Messages.ErrorMessageOperationNotSupportedOnPlatform);
            }
#endif

            var returnValue = new List<ConnectedDeviceDefinition>();

            //TODO: Logging

            var registryAvailable = false;

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
                if (key != null)
                {
                    registryAvailable = true;

                    //We can look at the registry

                    var valueNames = key.GetValueNames();

                    returnValue.AddRange(from valueName in valueNames let comPortName = key.GetValue(valueName) select new ConnectedDeviceDefinition($@"\\.\{comPortName}", DeviceType.SerialPort, label: valueName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            if (registryAvailable) return returnValue;

            //We can't look at the registry so try connecting to the devices
            for (var i = 0; i < 9; i++)
            {
                var portName = $@"\\.\COM{i}";
                using var serialPortDevice = new WindowsSerialPortDevice(portName);
                await serialPortDevice.InitializeAsync(cancellationToken).ConfigureAwait(false);
                if (serialPortDevice.IsInitialized) returnValue.Add(new ConnectedDeviceDefinition(portName, DeviceType.SerialPort));
            }

            return new ReadOnlyCollection<ConnectedDeviceDefinition>(returnValue);
        }

        public Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition connectedDeviceDefinition, CancellationToken cancellationToken = default)
             => Task.FromResult<IDevice>(connectedDeviceDefinition == null
                ? throw new ArgumentNullException(nameof(connectedDeviceDefinition))
                : new WindowsSerialPortDevice(connectedDeviceDefinition.DeviceId));

        public Task<bool> SupportsDeviceAsync(ConnectedDeviceDefinition connectedDeviceDefinition, CancellationToken cancellationToken = default)
            => connectedDeviceDefinition != null ? Task.FromResult(connectedDeviceDefinition.DeviceType == DeviceType.SerialPort) :
            throw new ArgumentNullException(nameof(connectedDeviceDefinition));

        #endregion
    }
}
