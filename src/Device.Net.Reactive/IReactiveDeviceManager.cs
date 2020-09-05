using System;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public interface IReactiveDeviceManager
    {
        void QueueRequest(IRequest request);

        Task<TResponse> WriteAndReadAsync<TResponse>(IRequest request, Func<byte[], TResponse> convertFunc);

        void SelectDevice(ConnectedDevice connectedDevice);

        void Start();
    }
}