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
        #region Public Properties
        public DeviceType DeviceType => DeviceType.SerialPort;
        public ILogger Logger { get; }
        public ILoggerFactory LoggerFactory { get; }
        public ITracer Tracer { get; }
        #endregion

        #region Constructor
        public WindowsSerialPortDeviceFactory(ILoggerFactory loggerFactory, ITracer tracer)
        {
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            //Note this loggerfactory may get shared with other factories of this type
            Logger = loggerFactory.CreateLogger(nameof(WindowsSerialPortDeviceFactory));
            Tracer = tracer;
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
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
            catch
            {
                //TODO: Logging
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
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));

            return new WindowsSerialPortDevice(deviceDefinition.DeviceId);
        }

        [Obsolete(DeviceManager.ObsoleteMessage)]
        public static void Register(ILoggerFactory loggerFactory, ITracer tracer) => DeviceManager.Current.DeviceFactories.Add(new WindowsSerialPortDeviceFactory(loggerFactory, tracer));
        #endregion
    }
}
