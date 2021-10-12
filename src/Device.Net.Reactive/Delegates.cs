using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public delegate void DeviceNotify(IDevice connectedDevice);
    public delegate void NotifyDeviceException(ConnectedDeviceDefinition connectedDevice, Exception exception);
    public delegate Task<IReadOnlyList<ConnectedDeviceDefinition>> GetConnectedDevicesAsync();
}
