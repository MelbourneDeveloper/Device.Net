using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
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
        #region Fields
        private readonly ILogger _logger;
        #endregion

        #region Public Properties
        public UsbManager UsbManager { get; }
        public Context Context { get; }
        public ILoggerFactory LoggerFactory { get; }
        public ushort? ReadBufferSize { get; set; }
        public ushort? WriteBufferSize { get; set; }
        #endregion

        #region Public Static Properties
        public DeviceType DeviceType => DeviceType.Usb;
        #endregion

        #region Constructor
        public AndroidUsbDeviceFactory(UsbManager usbManager, Context context, ILoggerFactory loggerFactory)
        {
            UsbManager = usbManager ?? throw new ArgumentNullException(nameof(usbManager));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            //Warning: this may get used by other android factories
            _logger = LoggerFactory.CreateLogger<AndroidUsbDeviceFactory>();
        }
        #endregion

        #region Public Methods
        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return Task.Run<IEnumerable<ConnectedDeviceDefinition>>(() =>
            {
                //TODO: Get more details about the device.
                return deviceDefinition.VendorId.HasValue && deviceDefinition.ProductId.HasValue
                    ? UsbManager.DeviceList.Select(kvp => kvp.Value).Where(d => deviceDefinition.VendorId == d.VendorId && deviceDefinition.ProductId == d.ProductId).Select(GetAndroidDeviceDefinition).ToList()
                    : deviceDefinition.VendorId.HasValue
                    ? UsbManager.DeviceList.Select(kvp => kvp.Value).Where(d => deviceDefinition.VendorId == d.VendorId).Select(GetAndroidDeviceDefinition).ToList()
                    : UsbManager.DeviceList.Select(kvp => kvp.Value).Select(GetAndroidDeviceDefinition).ToList();
            });
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));

            if (!int.TryParse(deviceDefinition.DeviceId, out var deviceId))
            {
                throw new DeviceException($"The device Id '{deviceDefinition.DeviceId}' is not a valid integer");
            }

            return new UsbDevice(deviceDefinition.DeviceId, new AndroidUsbInterfaceManager(UsbManager, Context, deviceId, LoggerFactory.CreateLogger<UsbDevice>(), ReadBufferSize, WriteBufferSize), _logger);
        }
        #endregion

        #region Public Static Methods
        public static ConnectedDeviceDefinition GetAndroidDeviceDefinition(global::Android.Hardware.Usb.UsbDevice usbDevice)
        {
            if (usbDevice == null) throw new ArgumentNullException(nameof(usbDevice));

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
        #endregion
    }
}
