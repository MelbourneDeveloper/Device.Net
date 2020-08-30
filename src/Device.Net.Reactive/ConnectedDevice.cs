namespace Device.Net.Reactive
{
    public class ConnectedDevice
    {
        public string DeviceId { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ConnectedDevice connectedDevice
                ? string.Compare(DeviceId, connectedDevice.DeviceId, System.StringComparison.Ordinal) == 0
                : base.Equals(obj);
        }

    }
}
