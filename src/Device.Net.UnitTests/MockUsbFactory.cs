using System;

namespace Device.Net.UnitTests
{
    public class MockUsbFactory : MockFactoryBase, IDeviceFactory
    {
        public MockUsbFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }

        public override string DeviceId => MockUsbDevice.MockedDeviceId;

        public static bool IsConnectedStatic { get; set; }

        public override DeviceType DeviceType => DeviceType.Usb;

        public override bool IsConnected => IsConnectedStatic;

        public override uint ProductId => MockUsbDevice.ProductId;

        public override uint VendorId => MockUsbDevice.VendorId;

        public const string FoundMessage = "Found device {0}";

        public static void Register(ILogger logger, ITracer tracer)
        {
            DeviceManager.Current.DeviceFactories.Add(new MockUsbFactory(logger, tracer));
        }

        public override IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition != null)
            {
                if (deviceDefinition.DeviceId == DeviceId)
                {
                    if (!deviceDefinition.DeviceType.HasValue || deviceDefinition.DeviceType == DeviceType.Usb)
                    {
                        Logger?.Log(string.Format(FoundMessage, DeviceId), nameof(MockUsbFactory), null, LogLevel.Information);

                        return new MockUsbDevice(Logger, Tracer);
                    }
                }
            }

            throw new Exception("Couldn't get a device");
        }
    }
}
