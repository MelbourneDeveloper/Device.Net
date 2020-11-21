using System;
using System.Collections.Generic;

namespace Device.Net.Reactive
{
    public delegate void DevicesNotify(IReadOnlyCollection<ConnectedDeviceDefinition> connectedDevices);
    public delegate void DeviceNotify(ConnectedDeviceDefinition connectedDevice);
    public delegate void NotifyDeviceException(ConnectedDeviceDefinition connectedDevice, Exception exception);
}
