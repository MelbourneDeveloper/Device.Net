using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
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
            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", deviceId, nameof(GetDeviceDefinition));

                using (var safeFileHandle = HidService.CreateReadConnection(deviceId, FileAccessRights.None))
                {
                    if (safeFileHandle.IsInvalid) throw new DeviceException($"{nameof(HidService.CreateReadConnection)} call with Id of {deviceId} failed.");

                    Logger?.LogDebug(Messages.InformationMessageFoundDevice);

                    return HidService.GetDeviceDefinition(deviceId, safeFileHandle);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageCouldntGetDevice);
                return null;
            }
            finally
            {
                logScope?.Dispose();
            }
        }

        protected override Guid GetClassGuid() => HidService.GetHidGuid();

        #endregion

        #region Public Properties
        public IHidApiService HidService { get; }
        #endregion

        #region Constructor
        public WindowsHidDeviceFactory(ILoggerFactory loggerFactory, ITracer tracer) : this(loggerFactory, tracer, null)
        {

        }

        public WindowsHidDeviceFactory(ILoggerFactory loggerFactory, ITracer tracer, IHidApiService hidService) : base(loggerFactory, tracer)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            HidService = hidService;

            if (HidService == null)
            {
                HidService = new WindowsHidApiService(loggerFactory.CreateLogger(nameof(WindowsHidApiService)));
            }
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));

            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsHidDevice(deviceDefinition.DeviceId, LoggerFactory.CreateLogger(nameof(WindowsHidDevice)), Tracer);
        }
        #endregion

        #region Private Static Methods

        #endregion

        #region Public Static Methods
        /// <summary>
        /// Register the factory for enumerating Hid devices on UWP. 
        /// </summary>
        [Obsolete(DeviceManager.ObsoleteMessage)]
        public static void Register(ILoggerFactory loggerFactory, ITracer tracer) => DeviceManager.Current.DeviceFactories.Add(new WindowsHidDeviceFactory(loggerFactory, tracer));
        #endregion
    }
}
