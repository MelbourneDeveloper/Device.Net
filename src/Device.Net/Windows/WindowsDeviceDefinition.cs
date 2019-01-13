namespace Device.Net.Windows
{
    public class WindowsDeviceDefinition : DeviceDefinitionPlus
    {
        public WindowsDeviceDefinition(string deviceId) : base(deviceId)
        {
        }

        public ushort? Usage { get; set; }
        public ushort? VersionNumber { get; set; }
    }
}
