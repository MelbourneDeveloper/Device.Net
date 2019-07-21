using System;

namespace Device.Net.Exceptions
{
    public class DeviceException : Exception
    {
        public DeviceException(string message) : base(message)
        {
        }

        public DeviceException()
        {
        }

        public DeviceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
