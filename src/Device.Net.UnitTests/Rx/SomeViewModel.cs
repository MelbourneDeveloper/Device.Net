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

        public event PropertyChangedEventHandler PropertyChanged;

        public ImmutableList<DeviceDescription> DeviceDescriptions { get; private set; }


        public SomeViewModel(IDeviceManager deviceManager)
        {
            this.deviceManager = deviceManager;
            var observer = new Observer<IReadOnlyCollection<ConnectedDeviceDefinition>>(DevicesListed);
            deviceManagerSubscription = deviceManager.ConnectedDevicesObservable.Subscribe(observer);
        }

        private void DevicesListed(IReadOnlyCollection<ConnectedDeviceDefinition> devices)
        {
            DeviceDescriptions = devices.Select(d => new DeviceDescription { Description = d.DeviceId }).ToImmutableList();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceDescriptions)));
        }

        //TODO: Verify unsubscribe on the observable
        public void Dispose() => deviceManagerSubscription.Dispose();
    }

    public class DeviceDescription
    {
        public string Description { get; set; }
    }
}
