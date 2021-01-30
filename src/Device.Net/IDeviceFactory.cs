using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary>
    /// Abstraction for enumerating and constructing <see cref="IDeviceFactory"/>s 
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        /// Gets the definition of connected devices
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Given a <see cref="ConnectedDeviceDefinition"/> returns a <see cref="IDevice"/>
        /// </summary>
        /// <param name="connectedDeviceDefinition"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition connectedDeviceDefinition, CancellationToken cancellationToken = default);

        /// <summary>
        /// Whether or not the factory supports the given device definition
        /// </summary>
        /// <param name="connectedDeviceDefinition"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> SupportsDeviceAsync(ConnectedDeviceDefinition connectedDeviceDefinition, CancellationToken cancellationToken = default);
    }
}
