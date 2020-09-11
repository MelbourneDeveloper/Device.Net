#if !NET45

using System;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public class IntegrationTester
    {
        private readonly IDevice _device;

        public IntegrationTester(IDevice device)
        {
            _device = device;
        }

        public async Task TestAsync(byte[] writeData, Func<byte[], Task> assertFunc)
        {
            var result = await _device.WriteAndReadAsync(writeData);
            await assertFunc(result.Data);
        }
    }
}

#endif
