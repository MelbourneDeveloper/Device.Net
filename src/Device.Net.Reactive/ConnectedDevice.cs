using System.Collections.Generic;

namespace Device.Net.Reactive
{
    public class ConnectedDevice
    {
        public string DeviceId { get; set; }

        public override bool Equals(object obj)
        {
            var returnValue = obj is ConnectedDevice connectedDevice
                ? string.Compare(DeviceId, connectedDevice.DeviceId, System.StringComparison.Ordinal) == 0
                : base.Equals(obj);

            return returnValue;
        }

        public override int GetHashCode() => -693647698 + EqualityComparer<string>.Default.GetHashCode(DeviceId);
    }
}
