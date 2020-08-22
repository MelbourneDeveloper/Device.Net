using Microsoft.Extensions.Logging;

namespace Device.Net.UnitTests
{
    public class MockHidFactory : MockFactoryBase
    {
        public const string FoundMessage = "Found device {0}";

        public MockHidFactory(ILogger logger) : base(logger)
        {
        }

        public override string DeviceId => MockHidDevice.MockedDeviceId;

        public static bool IsConnectedStatic { get; set; }

        public override DeviceType DeviceType => DeviceType.Hid;

        public override bool IsConnected => IsConnectedStatic;

        public override uint ProductId => MockHidDevice.ProductId;

        public override uint VendorId => MockHidDevice.VendorId;

        public override IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) return null;

            if (deviceDefinition.DeviceId != DeviceId) return null;

            if (deviceDefinition.DeviceType.HasValue && deviceDefinition.DeviceType != DeviceType.Hid) return null;

            Logger?.LogInformation(string.Format(FoundMessage, DeviceId));

            return new MockHidDevice(DeviceId, Logger);
        }
    }
}
