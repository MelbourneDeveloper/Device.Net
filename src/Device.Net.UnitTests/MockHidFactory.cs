using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public class MockHidFactory : IDeviceFactory
    {
        private string DeviceId => MockHidDevice.MockedDeviceId;

        //private bool IsConnected => DateTime.Now.Minute % 2 == 0;
        public static bool IsConnected { get; set; }

        public DeviceType DeviceType => DeviceType.Hid;

        public ILogger Logger { get; private set; }

        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            var result = new List<ConnectedDeviceDefinition>();

            if (IsConnected)
            {
                Console.WriteLine("I'm saying the device is connected...");

                result.Add(new ConnectedDeviceDefinition(DeviceId) { ProductId = MockHidDevice.ProductId, VendorId = MockHidDevice.VendorId });
            }
            else
            {
                Console.WriteLine("I'm saying no device...");
            }

            return Task.FromResult<IEnumerable<ConnectedDeviceDefinition>>(result);
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceId == DeviceId)
            {
                return new MockHidDevice();
            }

            throw new Exception("Couldn't get a device");
        }

        public static void Register(ILogger logger)
        {
            DeviceManager.Current.DeviceFactories.Add(new MockHidFactory() { Logger = logger });
        }
    }
}
