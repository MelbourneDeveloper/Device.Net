using System;

namespace Device.Net.Exceptions
{
    public class DeviceFactoriesNotRegisteredException : Exception
    {
        public DeviceFactoriesNotRegisteredException() : base(Messages.ErrorMessageNoDeviceFactoriesRegistered)
        {
        }

        public DeviceFactoriesNotRegisteredException(string message) : base(message)
        {
        }

        public DeviceFactoriesNotRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}