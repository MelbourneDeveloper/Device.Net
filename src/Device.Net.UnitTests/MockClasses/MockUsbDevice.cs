namespace Device.Net.UnitTests
{
    public class MockUsbDevice : MockDeviceBase
    {
        public const uint ProductId = 2;
        public const uint VendorId = 2;
        public const string MockedDeviceId = "321";

        public MockUsbDevice(string deviceId, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
            ConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId) { ProductId = ProductId, VendorId = VendorId, DeviceType = DeviceType.Usb };
        }
    }
}

