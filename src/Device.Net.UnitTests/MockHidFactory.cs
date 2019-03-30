using System;

namespace Device.Net.UnitTests
{
    public class MockHidFactory : MockFactoryBase, IDeviceFactory
    {
        public override string DeviceId => MockHidDevice.MockedDeviceId;

        public static bool IsConnectedStatic { get; set; }

        public override DeviceType DeviceType => DeviceType.Hid;

        public override bool IsConnected => IsConnectedStatic;

        public override uint ProductId => MockHidDevice.ProductId;

        public override uint VendorId => MockHidDevice.VendorId;

        public static void Register(ILogger logger)
        {
            DeviceManager.Current.DeviceFactories.Add(new MockHidFactory() { Logger = logger });
        }

        public override IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition != null)
            {
                if (deviceDefinition.DeviceId == DeviceId)
                {
                    if (!deviceDefinition.DeviceType.HasValue || deviceDefinition.DeviceType == DeviceType.Hid) return new MockHidDevice();
                }
            }

            throw new Exception("Couldn't get a device");
        }
    }
}
