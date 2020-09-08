using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public abstract class MockFactoryBase : IDeviceFactory
    {
        private readonly FilterDeviceDefinition _deviceDefinition;
        private readonly DeviceType _deviceType;

        public MockFactoryBase(FilterDeviceDefinition deviceDefinitions, DeviceType deviceType)
        {
            _deviceDefinition = deviceDefinitions;
            _deviceType = deviceType;
        }

        public abstract bool IsConnected { get; }

        public abstract string DeviceId { get; }

        public abstract DeviceType DeviceType { get; }

        public ILogger Logger { get; }

        protected MockFactoryBase(ILogger logger)
        {
            Logger = logger;
        }

        public abstract uint ProductId { get; }
        public abstract uint VendorId { get; }

        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
        {
            var result = new List<ConnectedDeviceDefinition>();

            var mockConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId)
            {
                ProductId = ProductId,
                VendorId = VendorId,
                DeviceType = this is MockHidFactory ? DeviceType.Hid : DeviceType.Usb
            };


            if (!IsConnected) return Task.FromResult<IEnumerable<ConnectedDeviceDefinition>>(result);

            if (DeviceManager.IsDefinitionMatch(_deviceDefinition, mockConnectedDeviceDefinition, _deviceType))
            {
                result.Add(mockConnectedDeviceDefinition);
            }

            return Task.FromResult<IEnumerable<ConnectedDeviceDefinition>>(result);
        }

        public abstract Task<IDevice> GetDevice(ConnectedDeviceDefinition deviceDefinition);
    }
}
