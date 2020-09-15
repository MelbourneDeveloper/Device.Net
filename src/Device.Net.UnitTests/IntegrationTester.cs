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
            IDeviceFactory deviceFactory,
            //TODO: Mock this
            ILoggerFactory loggerFactory
            )
        {
            _deviceFactory = deviceFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task TestAsync(byte[] writeData, Func<ReadResult, IDevice, Task> assertFunc)
        {
            var deviceManager = new DeviceManager(_loggerFactory);
            deviceManager.DeviceFactories.Add(_deviceFactory);

            var devices = await deviceManager.GetConnectedDeviceDefinitionsAsync();

            //Get the first available device
            var deviceDefinition = devices.FirstOrDefault();

            //Ensure that it gets picked up
            Assert.IsNotNull(deviceDefinition);

            //Initialize the device
            await deviceDefinition.InitializeAsync();

            var result = await deviceDefinition.WriteAndReadAsync(writeData);
            await assertFunc(result, deviceDefinition);
        }
    }
}

#endif
