using System;
using System.Runtime.Serialization;

namespace Device.Net
{
    [Serializable]
    public class DeviceFactoriesNotRegisteredException : Exception
    {
        public DeviceFactoriesNotRegisteredException()
        {
        }

        public DeviceFactoriesNotRegisteredException(string message) : base(message)
        {
        }

        public DeviceFactoriesNotRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeviceFactoriesNotRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}