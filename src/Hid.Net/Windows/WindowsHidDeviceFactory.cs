using Device.Net;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using System;

namespace Hid.Net.Windows
{


    public static class WindowsHidDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateWindowsHidDeviceFactory(ILoggerFactory loggerFactory)
        {
            //var asdasd = new ASDasdasd(loggerFactory.CreateLogger<ASDasdasd>());


            var asdasd = new WindowsDeviceEnumerator();

            return CreateWindowsHidDeviceFactory(asdasd.GetConnectedDeviceDefinitionsAsync, (c) => new WindowsHidDevice(c.DeviceId, loggerFactory), loggerFactory);
        }

        public static IDeviceFactory CreateWindowsHidDeviceFactory(this GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync, GetDevice getDevice, ILoggerFactory loggerFactory)
        {
            return getConnectedDeviceDefinitionsAsync == null
                ? throw new ArgumentNullException(nameof(getConnectedDeviceDefinitionsAsync))
                : getDevice == null
                ? throw new ArgumentNullException(nameof(getDevice))
                : loggerFactory == null
                ? throw new ArgumentNullException(nameof(loggerFactory))
                : new DeviceFactory(loggerFactory, getConnectedDeviceDefinitionsAsync, getDevice);
        }
    }

    internal class ASDasdasd
    {
        private readonly ILogger Logger;

        internal ASDasdasd(ILogger logger)
        {
            Logger = logger;
        }

        #region Protected Override Methods
        //public ConnectedDeviceDefinition GetDeviceDefinition(string deviceId)
        //{
        //    IDisposable logScope = null;

        //    try
        //    {
        //        logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", deviceId, nameof(GetDeviceDefinition));

        //        using (var safeFileHandle = HidService.CreateReadConnection(deviceId, FileAccessRights.None))
        //        {
        //            if (safeFileHandle.IsInvalid) throw new DeviceException($"{nameof(HidService.CreateReadConnection)} call with Id of {deviceId} failed.");

        //            Logger?.LogDebug(Messages.InformationMessageFoundDevice);

        //            return HidService.GetDeviceDefinition(deviceId, safeFileHandle);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger?.LogError(ex, Messages.ErrorMessageCouldntGetDevice);
        //        return null;
        //    }
        //    finally
        //    {
        //        logScope?.Dispose();
        //    }
        //}

        //protected override Guid GetClassGuid() => HidService.GetHidGuid();

        #endregion

        //#region Public Properties
        //public IHidApiService HidService { get; }
        //#endregion

        //#region Constructor
        //public WindowsHidDeviceFactory(
        //    ILoggerFactory loggerFactory,
        //    GetConnectedDevicesAsync getConnectedDevicesAsync) : this(
        //        loggerFactory,
        //        null,
        //        getConnectedDevicesAsync)
        //{

        //}

        //public WindowsHidDeviceFactory(
        //    ILoggerFactory loggerFactory,
        //    IHidApiService hidService,
        //    GetConnectedDevicesAsync getConnectedDevicesAsync) : base(
        //        loggerFactory,
        //        loggerFactory.CreateLogger<WindowsHidDeviceFactory>(),
        //        getConnectedDevicesAsync)
        //{
        //    if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

        //    HidService = hidService;

        //    if (HidService == null)
        //    {
        //        HidService = new WindowsHidApiService(loggerFactory);
        //    }
        //}
        //#endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition) => deviceDefinition == null
                ? throw new ArgumentNullException(nameof(deviceDefinition))
                : deviceDefinition.DeviceType != DeviceType ? null : new WindowsHidDevice(deviceDefinition.DeviceId, LoggerFactory);
        #endregion

        #region Private Static Methods

        #endregion
    }
}
