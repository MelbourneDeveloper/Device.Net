using System;
using System.Collections.Generic;

namespace Device.Net.Reactive
{
    public delegate void DevicesNotify(IReadOnlyCollection<ConnectedDevice> connectedDevices);
    public delegate void DeviceNotify(ConnectedDevice connectedDevice);
    public delegate void NotifyDeviceException(ConnectedDevice connectedDevice, Exception exception);
}
