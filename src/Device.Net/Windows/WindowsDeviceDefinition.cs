namespace Device.Net.Windows
{
    public class WindowsDeviceDefinition : DeviceDefinition
    {
        public WindowsDeviceDefinition(string deviceId) : base(deviceId)
        {
        }

        public ushort? Usage { get; set; }
        public ushort? UsagePage { get; set; }
        public ushort? VersionNumber { get; set; }
    }
}
