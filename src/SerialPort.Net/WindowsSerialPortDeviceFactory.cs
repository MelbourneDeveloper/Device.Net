using Device.Net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        public DeviceType DeviceType => DeviceType.SerialPort;
        #endregion

        #region Constructor
        public WindowsSerialPortDeviceFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            //Note this loggerfactory may get shared with other factories of this type
            _logger = _loggerFactory.CreateLogger<WindowsSerialPortDeviceFactory>();
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
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
                using (var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM"))
                {
                    if (key != null)
                    {
                        registryAvailable = true;

                        //We can look at the registry

                        var valueNames = key.GetValueNames();

                        foreach (var valueName in valueNames)
                        {
                            var comPortName = key.GetValue(valueName);
                            returnValue.Add(new ConnectedDeviceDefinition($@"\\.\{comPortName}") { Label = valueName });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
            }

            if (!registryAvailable)
            {
                //We can't look at the registry so try connecting to the devices
                for (var i = 0; i < 9; i++)
                {
                    var portName = $@"\\.\COM{i}";
                    using (var serialPortDevice = new WindowsSerialPortDevice(portName))
                    {
                        await serialPortDevice.InitializeAsync();
                        if (serialPortDevice.IsInitialized) returnValue.Add(new ConnectedDeviceDefinition(portName));
                    }
                }
            }

            return returnValue;
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return deviceDefinition == null
                ? throw new ArgumentNullException(nameof(deviceDefinition))
                : new WindowsSerialPortDevice(deviceDefinition.DeviceId);
        }
        #endregion
    }
}
