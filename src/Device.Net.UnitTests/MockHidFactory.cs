using System;

namespace Device.Net.UnitTests
{
    public class MockHidFactory : MockFactoryBase, IDeviceFactory
    {
        public const string FoundMessage = "Found device {0}";

        public MockHidFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }

        public override string DeviceId => MockHidDevice.MockedDeviceId;

        public static bool IsConnectedStatic { get; set; }

        public override DeviceType DeviceType => DeviceType.Hid;

        public override bool IsConnected => IsConnectedStatic;

        public override uint ProductId => MockHidDevice.ProductId;

        public override uint VendorId => MockHidDevice.VendorId;

        public static void Register(ILogger logger, ITracer tracer)
        {
            DeviceManager.Current.DeviceFactories.Add(new MockHidFactory(logger, tracer));
        }

        public override IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition != null)
            {
                if (deviceDefinition.DeviceId == DeviceId)
                {
                    if (!deviceDefinition.DeviceType.HasValue || deviceDefinition.DeviceType == DeviceType.Hid)
                    {
                        Logger?.Log(string.Format(FoundMessage, DeviceId), nameof(MockHidFactory), null, LogLevel.Information);

                        return new MockHidDevice(Logger, Tracer);
                    }
                }
            }

            return null;
        }
    }
}
