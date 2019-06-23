using System;

namespace Device.Net.UnitTests
{
    public class MockHidFactory : MockFactoryBase, IDeviceFactory
    {
        public MockHidFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }

        public override string DeviceId => MockHidDevice.MockedDeviceId;

        public static bool IsConnectedStatic { get; set; }

        public override DeviceType DeviceType => DeviceType.Hid;

        public override bool IsConnected => IsConnectedStatic;

        public override uint ProductId => MockHidDevice.ProductId;

        public override uint VendorId => MockHidDevice.VendorId;

        public static void Register(ILogger logger, ITracer trace)
        {
            DeviceManager.Current.DeviceFactories.Add(new MockHidFactory(logger, trace));
        }

        public override IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition != null)
            {
                if (deviceDefinition.DeviceId == DeviceId)
                {
                    if (!deviceDefinition.DeviceType.HasValue || deviceDefinition.DeviceType == DeviceType.Hid) return new MockHidDevice(Logger, Tracer);
                }
            }

            throw new Exception("Couldn't get a device");
        }
    }
}
