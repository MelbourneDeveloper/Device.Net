using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public abstract class MockFactoryBase : IDeviceFactory
    {
        public abstract bool IsConnected { get; }

        public abstract string DeviceId { get; }

        public abstract DeviceType DeviceType { get; }

        public ILogger Logger { get; }
        public ITracer Tracer { get; }

        protected MockFactoryBase(ILogger logger, ITracer tracer)
        {
            Logger = logger;
            Tracer = tracer;
        }

        public abstract uint ProductId { get; }
        public abstract uint VendorId { get; }

        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            var result = new List<ConnectedDeviceDefinition>();

            var mockConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId) { ProductId = ProductId, VendorId = VendorId, DeviceType = deviceDefinition.DeviceType };

            if (IsConnected)
            {
                if (DeviceManager.IsDefinitionMatch(deviceDefinition, mockConnectedDeviceDefinition))
                {
                    result.Add(mockConnectedDeviceDefinition);
                }
            }

            return Task.FromResult<IEnumerable<ConnectedDeviceDefinition>>(result);
        }

        public abstract IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition);
    }
}
