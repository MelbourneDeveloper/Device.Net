using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public class IntegrationTester
    {
        private readonly IDeviceFactory _deviceFactory;

        public IntegrationTester(
            IDeviceFactory deviceFactory) => _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));

        public async Task<IDevice> TestAsync(
            byte[] writeData,
            Func<TransferResult, IDevice, Task> assertFunc,
            int expectedDataLength,
            //This is for Hid only. Because for several devices we send/receive 65, but end up with 64 bytes
            uint? expectedTransferLength = null,
            bool dispose = true)
        {
            //Get it from the data length unless it's Hid
            expectedTransferLength ??= (uint)expectedDataLength;

            var devices = await _deviceFactory.GetConnectedDeviceDefinitionsAsync();

            //Get the first available device
            var deviceDefinition = devices.FirstOrDefault();

            //Ensure that it gets picked up
            Assert.IsNotNull(deviceDefinition);

            var device = await _deviceFactory.GetDeviceAsync(deviceDefinition);

            //Initialize the device
            await device.InitializeAsync();

            //The idea of this is to cancel any operation if the whole thing takes more than a second
            //However, this basically doesn't work. Some operations simply don't support cancellation tokens
            //This more or less seems like a bug.
            //See issue: https://github.com/MelbourneDeveloper/Device.Net/issues/188
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            var result = await device.WriteAndReadAsync(writeData, cancellationTokenSource.Token);

            Assert.AreEqual(expectedTransferLength.Value, result.BytesTransferred);
            Assert.AreEqual(expectedDataLength, result.Data.Length);

            await assertFunc(result, device);

            if (dispose) device.Dispose();

            return device;
        }
    }
}
