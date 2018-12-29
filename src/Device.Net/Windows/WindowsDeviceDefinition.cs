using Device.Net;

namespace Device.Net.Windows
{
    public class WindowsDeviceDefinition : DeviceDefinition
    {
        public ushort Usage { get; set; }
        public ushort UsagePage { get; set; }
        public ushort VersionNumber { get; set; }
    }
}
