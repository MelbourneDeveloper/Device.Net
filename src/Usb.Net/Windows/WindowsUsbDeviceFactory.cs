using Device.Net;
using Device.Net.Windows;
using System;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Usb;
        #endregion

        #region Protected Override Methods

        /// <summary>
        /// The parent Guid to enumerate through devices at. This probably shouldn't be changed. It defaults to the WinUSB Guid. 
        /// </summary>
        protected override Guid GetClassGuid() => WindowsDeviceConstants.WinUSBGuid;
        #endregion

        #region Constructor
        public WindowsUsbDeviceFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsUsbDevice(deviceDefinition.DeviceId, Logger, Tracer);
        }
        #endregion

        #region Private Static Methods
        protected override ConnectedDeviceDefinition GetDeviceDefinition(string deviceId)
        {
            return GetDeviceDefinitionFromWindowsDeviceId(deviceId, DeviceType.Usb);
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Register the factory for enumerating USB devices on UWP. Warning: no tracing or logging will be used. Please user the other constructor overload for logging and tracing
        /// </summary>
        public static void Register()
        {
            Register(null, null);
        }

        /// <summary>
        /// Register the factory for enumerating USB devices on UWP.
        /// </summary>
        public static void Register(ILogger logger, ITracer tracer)
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsUsbDeviceFactory(logger, tracer));
        }
        #endregion
    }
}
