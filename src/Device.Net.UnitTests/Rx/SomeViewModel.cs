
#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;


namespace Device.Net.UnitTests
{
    public class SomeViewModel : INotifyPropertyChanged, IDisposable
    {

        private readonly IDisposable deviceManagerSubscription;
        private readonly IDisposable connectionSubscription;
        private readonly IDeviceManager deviceManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImmutableList<DeviceDescription> DeviceDescriptions { get; private set; } = ImmutableList<DeviceDescription>.Empty;
        public IDevice ConnectedDevice => deviceManager.SelectedDevice;


        public SomeViewModel(IDeviceManager deviceManager, IObservable<IDevice> connectedDeviceStream)
        {
            var observer = new Observer<IReadOnlyCollection<ConnectedDeviceDefinition>>(DevicesListed);
            var observer2 = new Observer<IDevice>(ConnectionStatusChanged);
            deviceManagerSubscription = deviceManager.ConnectedDevicesObservable.Subscribe(observer);
            connectionSubscription = connectedDeviceStream.Subscribe(observer2);
            this.deviceManager = deviceManager;
        }

        private void ConnectionStatusChanged(IDevice device) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectedDevice)));

        private void DevicesListed(IReadOnlyCollection<ConnectedDeviceDefinition> devices)
        {
            DeviceDescriptions = devices.Select(d => new DeviceDescription(d)).ToImmutableList();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceDescriptions)));

            if (DeviceDescriptions.Count > 0 &&
                (deviceManager?.SelectedDevice == null ||
                deviceManager?.SelectedDevice?.ConnectedDeviceDefinition?.DeviceId != devices.FirstOrDefault()?.DeviceId))
            {
                Debug.WriteLine($"Call SelectDevice Is Null: {deviceManager?.SelectedDevice == null} 1: {deviceManager?.SelectedDevice?.ConnectedDeviceDefinition?.DeviceId} 2: {devices.FirstOrDefault()?.DeviceId}");

                //A valid device was connected so select it
                deviceManager.SelectDevice(devices.First());
            }
        }

        //TODO: Verify unsubscribe on the observable
        public void Dispose()
        {
            connectionSubscription.Dispose();
            deviceManagerSubscription.Dispose();
        }


    }

    public class DeviceDescription
    {
        private readonly ConnectedDeviceDefinition connectedDeviceDefinition;

        public DeviceDescription(ConnectedDeviceDefinition connectedDeviceDefinition)
            => this.connectedDeviceDefinition = connectedDeviceDefinition;

        public string Description => connectedDeviceDefinition.DeviceId;
    }
}

#endif