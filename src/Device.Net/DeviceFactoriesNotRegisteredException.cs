using System;

namespace Device.Net
{
#pragma warning disable CA2229 
#pragma warning disable CA1032 
    public class DeviceFactoriesNotRegisteredException : Exception
#pragma warning restore CA1032 
#pragma warning restore CA2229 
    {
        public DeviceFactoriesNotRegisteredException() : base(Messages.ErrorMessageNoDeviceFactoriesRegistered)
        {
        }
    }
}