namespace Device.Net.UnitTests
{
    public class MockUsbDevice : MockDeviceBase, IDevice
    {
        public const uint ProductId = 2;
        public const uint VendorId = 2;
        public const string MockedDeviceId = "321";

        public MockUsbDevice(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
            DeviceId = MockedDeviceId;
            ConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId) { ProductId = ProductId, VendorId = VendorId };
        }
    }
}

