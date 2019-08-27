using System;

namespace Device.Net
{

    /// <summary>
    /// Represents the definition of a device that has been physically connected and has a DeviceId
    /// </summary>
    public class ConnectedDeviceDefinition : ConnectedDeviceDefinitionBase
    {
        #region Public Methods
        public string DeviceId { get; }
        #endregion

        #region Constructor
        public ConnectedDeviceDefinition(string deviceId)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            DeviceId = deviceId;
        }
        #endregion
    }
}