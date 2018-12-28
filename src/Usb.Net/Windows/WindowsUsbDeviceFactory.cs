using Device.Net;
using Device.Net.Windows;
using System;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Usb;
        //TODO: This is not right
        public override Guid ClassGuid { get; set; } = WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE;
        #endregion

        #region Public Methods
        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsUsbDevice(deviceDefinition.DeviceId);
        }
        #endregion

        #region Private Static Methods
        protected override DeviceDefinition GetDeviceDefinition(string deviceId)
        {
            return new DeviceDefinition { DeviceId = deviceId, DeviceType = DeviceType.Usb };
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsUsbDeviceFactory());
        }
        #endregion
    }
}
