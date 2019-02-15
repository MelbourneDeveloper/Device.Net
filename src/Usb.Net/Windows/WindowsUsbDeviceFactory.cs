using Device.Net;
using Device.Net.Windows;
using System;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Usb;

        /// <summary>
        /// The parent Guid to enumerate through devices at. This probably shouldn't be changed. It defaults to the WinUSB Guid. 
        /// </summary>
        public override Guid ClassGuid { get; set; } = WindowsDeviceConstants.WinUSBGuid;
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsUsbDevice(deviceDefinition.DeviceId) { Logger = Logger };
        }
        #endregion

        #region Private Static Methods
        protected override ConnectedDeviceDefinition GetDeviceDefinition(string deviceId)
        {
            return GetDeviceDefinitionFromWindowsDeviceId(deviceId, DeviceType.Usb);
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            Register(null);
        }

        public static void Register(ILogger logger)
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsUsbDeviceFactory() { Logger = logger });
        }
        #endregion
    }
}
