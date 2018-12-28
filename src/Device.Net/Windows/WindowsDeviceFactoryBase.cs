using System;
using System.Collections.Generic;
using System.Text;

namespace Device.Net.Windows
{
    public abstract class WindowsDeviceFactoryBase
    {
        public abstract DeviceType DeviceType { get; }
        public abstract Guid ClassGuid { get; set; }
    }
}
