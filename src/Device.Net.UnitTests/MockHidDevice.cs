namespace Device.Net.UnitTests
{
    public class MockHidDevice : MockDeviceBase, IDevice
    {
        public const uint ProductId = 1;
        public const uint VendorId = 1;
        public const string MockedDeviceId = "123";

        public MockHidDevice(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
            DeviceId = MockedDeviceId;
            ConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId) { ProductId = ProductId, VendorId = VendorId };
        }
    }
}
