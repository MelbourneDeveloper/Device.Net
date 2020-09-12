#if !NET45

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public class IntegrationTester
    {
        private readonly FilterDeviceDefinition _filterDeviceDefinition;
        private readonly IDeviceFactory _deviceFactory;
        private readonly ILoggerFactory _loggerFactory;

        public IntegrationTester(
            FilterDeviceDefinition filterDeviceDefinition,
            IDeviceFactory deviceFactory,
            //TODO: Mock this
            ILoggerFactory loggerFactory
            )
        {
            _filterDeviceDefinition = filterDeviceDefinition;
            _deviceFactory = deviceFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task TestAsync(byte[] writeData, Func<byte[], IDevice, Task> assertFunc)
        {
            var deviceManager = new DeviceManager(_loggerFactory);
            deviceManager.DeviceFactories.Add(_deviceFactory);

            var devices = await deviceManager.GetDevicesAsync(new List<FilterDeviceDefinition>
            {
                _filterDeviceDefinition,
            });

            //Get the first available device
            var device = devices.FirstOrDefault();

            //Ensure that it gets picked up
            Assert.IsNotNull(device);

            //Initialize the device
            await device.InitializeAsync();

            var result = await device.WriteAndReadAsync(writeData);
            await assertFunc(result.Data, device);
        }
    }
}

#endif
