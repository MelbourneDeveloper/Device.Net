using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public interface IReactiveDeviceManager
    {
        Func<IObserver<IReadOnlyCollection<ConnectedDevice>>, IDisposable> SubscribeToConnectedDevices { get; }
        Task<TResponse> WriteAndReadAsync<TRequest, TResponse>(TRequest request, Func<byte[], TResponse> convertFunc) where TRequest : IRequest;
        /// <summary>
        /// TODO:
        /// This property shouldn't be necessary. For some reason, it is only possible to Subscribe once, so we subscribe in the constructor and expose this so that the methods can be called
        /// </summary>
        IObserver<ConnectedDevice> ConnectedDeviceObserver { get; }
        /// <summary>
        /// TODO: Remove this. It's another hack
        /// </summary>
        IObserver<ConnectedDevice> InitializedDeviceObserver { get; set; }
    }
}