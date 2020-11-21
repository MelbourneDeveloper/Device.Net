using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(CancellationToken cancellationToken = default);
        Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition deviceDefinition, CancellationToken cancellationToken = default);
        Task<bool> SupportsDeviceAsync(ConnectedDeviceDefinition deviceDefinition, CancellationToken cancellationToken = default);
    }
}
