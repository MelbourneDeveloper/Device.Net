using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    public delegate Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(CancellationToken cancellationToken = default);
    public delegate ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, Guid classGuid);
    public delegate Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition deviceId, CancellationToken cancellationToken = default);

}
