using System;

namespace Device.Net.UnitTests
{
    public class MockUsbFactory : MockFactoryBase, IDeviceFactory
    {
        public MockUsbFactory()
        {
            Logger = new DebugLogger { LogToConsole = true };
        }

        public override string DeviceId => MockUsbDevice.MockedDeviceId;

        public static bool IsConnectedStatic { get; set; }

        public override DeviceType DeviceType => DeviceType.Usb;

        public override bool IsConnected => IsConnectedStatic;

        public override uint ProductId => MockUsbDevice.ProductId;

        public override uint VendorId => MockUsbDevice.VendorId;

        public static void Register(ILogger logger)
        {
            DeviceManager.Current.DeviceFactories.Add(new MockUsbFactory() { Logger = logger });
        }

        public override IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition != null)
            {
                if (deviceDefinition.DeviceId == DeviceId)
                {
                    if (!deviceDefinition.DeviceType.HasValue || deviceDefinition.DeviceType == DeviceType.Usb) return new MockUsbDevice() ;
                }
            }

            throw new Exception("Couldn't get a device");
        }
    }
}
