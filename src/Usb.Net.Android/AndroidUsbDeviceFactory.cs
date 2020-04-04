using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Android
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes
    /// </summary>
    public class AndroidUsbDeviceFactory : IDeviceFactory
    {
        #region Public Properties
        public UsbManager UsbManager { get; }
        public Context Context { get; }
        public ILogger Logger { get; }
        public ITracer Tracer { get; }
        public ushort? ReadBufferSize { get; set; }
        public ushort? WriteBufferSize { get; set; }
        #endregion

        #region Public Static Properties
        public DeviceType DeviceType => DeviceType.Usb;
        #endregion

        #region Constructor
        public AndroidUsbDeviceFactory(UsbManager usbManager, Context context, ILogger logger, ITracer tracer)
        {
            UsbManager = usbManager;
            Context = context;
            Logger = logger;
            Tracer = tracer;
        }
        #endregion

        #region Public Methods
        protected void Log(string message, Exception ex)
        {
            var callerMemberName = "";
            Logger?.Log(message, $"{ nameof(AndroidUsbDeviceFactory)} - {callerMemberName}", ex, ex != null ? LogLevel.Error : LogLevel.Information);
        }

        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return Task.Run<IEnumerable<ConnectedDeviceDefinition>>(() =>
            {
                //TODO: Get more details about the device.
                if (deviceDefinition.VendorId.HasValue && deviceDefinition.ProductId.HasValue)
                    return UsbManager.DeviceList.Select(kvp => kvp.Value).Where(d => deviceDefinition.VendorId == d.VendorId && deviceDefinition.ProductId == d.ProductId).Select(GetAndroidDeviceDefinition).ToList();
                else if (deviceDefinition.VendorId.HasValue)
                    return UsbManager.DeviceList.Select(kvp => kvp.Value).Where(d => deviceDefinition.VendorId == d.VendorId).Select(GetAndroidDeviceDefinition).ToList();
                else
                    return UsbManager.DeviceList.Select(kvp => kvp.Value).Select(GetAndroidDeviceDefinition).ToList();

            });
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition)));

            if (!int.TryParse(deviceDefinition.DeviceId, out var deviceId))
            {
                throw new Exception($"The device Id '{deviceDefinition.DeviceId}' is not a valid integer");
            }

            return new UsbDevice(deviceDefinition.DeviceId, new AndroidUsbInterfaceManager(UsbManager, Context, deviceId, Logger, Tracer, ReadBufferSize, WriteBufferSize), Logger, Tracer);
        }
        #endregion

        #region Public Static Methods
        public static ConnectedDeviceDefinition GetAndroidDeviceDefinition(global::Android.Hardware.Usb.UsbDevice usbDevice)
        {
            var deviceId = usbDevice.DeviceId.ToString(Helpers.ParsingCulture);

            return new ConnectedDeviceDefinition(deviceId)
            {
                ProductName = usbDevice.ProductName,
                Manufacturer = usbDevice.ManufacturerName,
                SerialNumber = usbDevice.SerialNumber,
                ProductId = (uint)usbDevice.ProductId,
                VendorId = (uint)usbDevice.VendorId,
                DeviceType = DeviceType.Usb
            };
        }

        /// <summary>
        /// Register the factory for enumerating USB devices on Android.
        /// </summary>
        public static void Register(UsbManager usbManager, Context context, ILogger logger, ITracer tracer)
        {
            DeviceManager.Current.DeviceFactories.Add(new AndroidUsbDeviceFactory(usbManager, context, logger, tracer));
        }
        #endregion
    }
}
