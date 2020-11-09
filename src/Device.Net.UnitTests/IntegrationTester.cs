#if !NET45

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public class IntegrationTester
    {
        private readonly IDeviceFactory _deviceFactory;

        public IntegrationTester(
            IDeviceFactory deviceFactory) => _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));

        public async Task TestAsync(byte[] writeData, Func<ReadResult, IDevice, Task> assertFunc, int expectedDataLength)
        {
            var devices = await _deviceFactory.GetConnectedDeviceDefinitionsAsync();

            //Get the first available device
            var deviceDefinition = devices.FirstOrDefault();

            //Ensure that it gets picked up
            Assert.IsNotNull(deviceDefinition);

            var device = await _deviceFactory.GetDevice(deviceDefinition);

            //Initialize the device
            await device.InitializeAsync();

            var result = await device.WriteAndReadAsync(writeData);

            Assert.AreEqual((uint)expectedDataLength, result.BytesRead);
            Assert.AreEqual(expectedDataLength, result.Data.Length);

            await assertFunc(result, device);
        }
    }
}

#endif
