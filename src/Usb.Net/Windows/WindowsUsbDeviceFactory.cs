using Device.Net;
using Device.Net.Windows;
using System;
using System.Globalization;

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
        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsUsbDevice(deviceDefinition.DeviceId);
        }
        #endregion

        #region Private Static Methods
        protected override DeviceDefinition GetDeviceDefinition(string deviceId)
        {
            uint? vid = null;
            uint? pid = null;
            try
            {
                vid = GetNumberFromDeviceId(deviceId, "vid_");
                pid = GetNumberFromDeviceId(deviceId, "pid_");
            }
            catch (Exception)
            {
                //TODO: Logging
                //We really need the Vid/Pid here for polling etc. so not sure if swallowing errors it the way to go
            }

            return new DeviceDefinition { DeviceId = deviceId, DeviceType = DeviceType.Usb, VendorId = vid, ProductId = pid };
        }

        private static uint GetNumberFromDeviceId(string deviceId, string searchString)
        {
            var indexOfSearchString = deviceId.ToLower().IndexOf(searchString);
            string hexString = null;
            if (indexOfSearchString > -1)
            {
                hexString = deviceId.Substring(indexOfSearchString + searchString.Length, 4);
            }
            var numberAsInteger = uint.Parse(hexString, NumberStyles.HexNumber);
            return numberAsInteger;
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
