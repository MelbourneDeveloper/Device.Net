using System;

namespace Device.Net
{
    public class DeviceEventArgs : EventArgs
    {
        public IDevice Device { get; }

        public DeviceEventArgs(IDevice device) => Device = device;
    }
}
