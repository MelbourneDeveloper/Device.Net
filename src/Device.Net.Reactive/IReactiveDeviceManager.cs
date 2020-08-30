using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public interface IReactiveDeviceManager
    {
        IList<FilterDeviceDefinition> FilterDeviceDefinitions { get; }
        IDevice SelectedDevice { get; }
        Func<IObserver<IReadOnlyCollection<ConnectedDevice>>, IDisposable> SubscribeToConnectedDevices { get; }
        Task<TResponse> WriteAndReadAsync<TRequest, TResponse>(TRequest request, Func<byte[], TResponse> convertFunc) where TRequest : IRequest;
    }
}