using System;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public interface IReactiveDeviceManager
    {
        void QueueRequest(IRequest request);


        Task<TResponse> WriteAndReadAsync<TResponse>(IRequest request, Func<byte[], TResponse> convertFunc);
        /// <summary>
        /// TODO:
        /// This property shouldn't be necessary. For some reason, it is only possible to Subscribe once, so we subscribe in the constructor and expose this so that the methods can be called
        /// </summary>
        IObserver<ConnectedDevice> SelectedDeviceObserver { get; }
        /// <summary>
        /// TODO: Remove this. It's another hack
        /// </summary>
        IObserver<ConnectedDevice> InitializedDeviceObserver { get; set; }
    }
}