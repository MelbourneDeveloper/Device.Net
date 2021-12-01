using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public delegate void DevicesNotify(IReadOnlyCollection<ConnectedDeviceDefinition> connectedDevices);
    public delegate void DeviceNotify(IDevice connectedDevice);
    public delegate void NotifyDeviceError(ConnectedDeviceDefinition connectedDevice, Exception exception);
    public delegate Task<IReadOnlyList<ConnectedDeviceDefinition>> GetConnectedDevicesAsync();

    public interface IRequest
    {
        byte[] ToArray();
    }

    /// <summary>
    /// This interface is a work in progress. It is not production ready.
    /// </summary>
    public interface IDeviceManager
    {
        void QueueRequest(IRequest request);

        Task<TResponse> WriteAndReadAsync<TResponse>(IRequest request, Func<byte[], TResponse> convertFunc);

        void SelectDevice(ConnectedDeviceDefinition connectedDevice);

        IDevice SelectedDevice { get; }

        Task Start();

        IObservable<IReadOnlyCollection<ConnectedDeviceDefinition>> ConnectedDevicesObservable { get; }
    }
}