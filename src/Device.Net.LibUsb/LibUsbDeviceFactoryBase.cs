using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public abstract class LibUsbDeviceFactoryBase : IDeviceFactory
    {
        public abstract DeviceType DeviceType {get;}

        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            throw new NotImplementedException();
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            throw new NotImplementedException();
        }
    }
}
