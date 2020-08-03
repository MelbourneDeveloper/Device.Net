using Microsoft.Extensions.Logging;

namespace Device.Net.UnitTests
{
    public class MockHidDevice : MockDeviceBase
    {
        public const uint ProductId = 1;
        public const uint VendorId = 1;
        public const string MockedDeviceId = "123";

        public MockHidDevice(string deviceId, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
            ConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId) { ProductId = ProductId, VendorId = VendorId, DeviceType = DeviceType.Hid };
        }
    }
}
