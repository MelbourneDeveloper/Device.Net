using System;

namespace Device.Net
{
    public class DeviceException : Exception
    {
        public DeviceException(string message) : base(message)
        {
        }
    }
}
