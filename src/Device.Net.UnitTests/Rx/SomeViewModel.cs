
#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Device.Net.UnitTests
{
    public class SomeViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable deviceManagerSubscription;
        private readonly IDeviceManager deviceManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImmutableList<DeviceDescription> DeviceDescriptions { get; private set; } = ImmutableList<DeviceDescription>.Empty;


        public SomeViewModel(IDeviceManager deviceManager)
        {
            var observer = new Observer<IReadOnlyCollection<ConnectedDeviceDefinition>>(DevicesListed);
            deviceManagerSubscription = deviceManager.ConnectedDevicesObservable.Subscribe(observer);
            this.deviceManager = deviceManager;
        }

        private void DevicesListed(IReadOnlyCollection<ConnectedDeviceDefinition> devices)
        {
            DeviceDescriptions = devices.Select(d => new DeviceDescription(d)).ToImmutableList();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceDescriptions)));

            if (DeviceDescriptions.Count > 0 && deviceManager?.SelectedDevice?.ConnectedDeviceDefinition?.DeviceId != devices.FirstOrDefault()?.DeviceId)
            {
                //A valid device was connected so select it
                deviceManager.SelectDevice(devices.First());
            }
        }

        //TODO: Verify unsubscribe on the observable
        public void Dispose() => deviceManagerSubscription.Dispose();
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