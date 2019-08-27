using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using System;

namespace Hid.Net.Windows
{
    public class WindowsHidDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        #endregion

        #region Protected Override Methods
        protected override ConnectedDeviceDefinition GetDeviceDefinition(string deviceId)
        {
            try
            {
                using (var safeFileHandle = HidService.CreateReadConnection(deviceId, FileAccessRights.None))
                {
                    if (safeFileHandle.IsInvalid) throw new DeviceException($"{nameof(HidService.CreateReadConnection)} call with Id of {deviceId} failed.");

                    Logger?.Log($"Found device {deviceId}", nameof(WindowsHidDeviceFactory), null, LogLevel.Information);

                    return HidService.GetDeviceDefinition(deviceId, safeFileHandle);
                }
            }
            catch (Exception ex)
            {
                Logger?.Log($"{nameof(GetDeviceDefinition)} error. Device Id: {deviceId}", nameof(WindowsHidDeviceFactory), ex, LogLevel.Error);
                return null;
            }
        }

        protected override Guid GetClassGuid()
        {
            return HidService.GetHidGuid();
        }

        #endregion

        #region Public Properties
        public IHidService HidService { get; }
        #endregion

        #region Constructor
        public WindowsHidDeviceFactory(ILogger logger, ITracer tracer) : this(logger, tracer, null)
        {

        }

        public WindowsHidDeviceFactory(ILogger logger, ITracer tracer, IHidService hidService) : base(logger, tracer)
        {
            HidService = hidService;

            if (HidService == null)
            {
                HidService = new WindowsHidApiService(logger);
            }
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));

            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsHidDevice(deviceDefinition.DeviceId, Logger, Tracer);
        }
        #endregion

        #region Private Static Methods

        #endregion

        #region Public Static Methods
        /// <summary>
        /// Register the factory for enumerating Hid devices on UWP. 
        /// </summary>
        public static void Register(ILogger logger, ITracer tracer)
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsHidDeviceFactory(logger, tracer));
        }
        #endregion
    }
}
