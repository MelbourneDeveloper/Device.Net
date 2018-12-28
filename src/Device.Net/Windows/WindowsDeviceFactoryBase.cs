using System;
using System.Collections.Generic;
using System.Text;

namespace Device.Net.Windows
{
    public abstract class WindowsDeviceFactoryBase
    {
        #region Public Abstract Properties
        public abstract DeviceType DeviceType { get; }
        public abstract Guid ClassGuid { get; set; }
        #endregion

        #region Protected Abstract Methods
        protected abstract DeviceDefinition GetDeviceDefinition(string deviceId);
        #endregion
    }
}
