using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary>
    /// This interface is a work in progress. It is not production ready.
    /// </summary>
    public interface IDeviceManager
    {
        void QueueRequest(IRequest request);

        Task<TResponse> WriteAndReadAsync<TResponse>(IRequest request, Func<byte[], TResponse> convertFunc);

        void SelectDevice(ConnectedDeviceDefinition connectedDevice);

        IDevice SelectedDevice { get; }

        void Start();

        IObservable<IReadOnlyCollection<ConnectedDeviceDefinition>> ConnectedDevicesObservable { get; }
    }
}