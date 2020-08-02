using System;

namespace Device.Net.UnitTests
{
    public class MockUsbFactory : MockFactoryBase
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

        public override IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new Exception("Couldn't get a device");

            if (deviceDefinition.DeviceId != DeviceId) throw new Exception("Couldn't get a device");

            if (deviceDefinition.DeviceType.HasValue && deviceDefinition.DeviceType != DeviceType.Usb) throw new Exception("Couldn't get a device");

            Logger?.Log(string.Format(FoundMessage, DeviceId), nameof(MockUsbFactory), null, LogLevel.Information);

            return new MockUsbDevice(DeviceId, Logger, Tracer);
        }
    }
}
