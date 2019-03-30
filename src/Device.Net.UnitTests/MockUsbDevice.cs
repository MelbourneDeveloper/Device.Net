namespace Device.Net.UnitTests
{
    public class MockUsbDevice : MockDeviceBase, IDevice
    {
        public const uint ProductId = 2;
        public const uint VendorId = 2;
        public const string MockedDeviceId = "321";

        public MockUsbDevice()
        {
            DeviceId = MockedDeviceId;
            ConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId) { ProductId = ProductId, VendorId = VendorId };
        }
    }
}
