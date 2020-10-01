using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        IAsyncEnumerable<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionsAsync(Func<ValueTask<bool>> moveNextAsync = null);
        Task<IDevice> GetDevice(ConnectedDeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
    }

    /* EXAMPLE
     * 
     *             var ctSource = new CancellationTokenSource();
            var ct = ctSource.Token;

            var asyncEnumerable = AsyncEnumerable.Create((ct)
                =>
                AsyncEnumerator.Create(
                    async () => true,
                     () => new Random().Next(),
                    async () => { }
                    )
            ); ;

            var i = 0;

            //var asdasds = new Fadasdsf();
            await foreach (int item in asyncEnumerable)
            {
                if (i > 5)
                    break;

                i++;
            }
     * 
     */

    public static class Asdasdsd
    {
        public static async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(this IDeviceFactory deviceFactory)
            => deviceFactory == null ?
            throw new ArgumentNullException(nameof(deviceFactory))
            : await deviceFactory.GetConnectedDeviceDefinitionsAsync().ToListAsync();
    }
}
