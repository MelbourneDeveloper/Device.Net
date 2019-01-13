namespace Device.Net
{
    public class DeviceDefinitionPlus : DeviceDefinition
    {
        public string DeviceId { get; set; }

        public DeviceDefinitionPlus(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}