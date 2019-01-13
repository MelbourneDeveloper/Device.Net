using Android.Content;
using Android.Hardware.Usb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net.Android;

namespace Device.Net
{
    public class AndroidUsbDeviceFactory : IDeviceFactory
    {
        #region Public Properties
        public UsbManager UsbManager { get; }
        public Context Context { get; }
        #endregion

        #region Public Static Properties
        public DeviceType DeviceType => DeviceType.Usb;
        #endregion

        #region Constructor
        public AndroidUsbDeviceFactory(UsbManager usbManager, Context context)
        {
            UsbManager = usbManager;
            Context = context;
        }
        #endregion

        #region Public Methods
        public Task<IEnumerable<DeviceDefinitionPlus>> GetConnectedDeviceDefinitions(DeviceDefinition deviceDefinition)
        {
            return Task.Run<IEnumerable<DeviceDefinitionPlus>>(() =>
            {
                //TODO: Get more details about the device.
                return UsbManager.DeviceList.Select(kvp => kvp.Value).Where(d => deviceDefinition.VendorId == d.VendorId && deviceDefinition.ProductId == d.ProductId).Select(GetAndroidDeviceDefinition).ToList();
            });
        }

        public IDevice GetDevice(DeviceDefinitionPlus deviceDefinition)
        {
            if (!int.TryParse(deviceDefinition.DeviceId, out var deviceId))
            {
                throw new Exception($"The device Id '{deviceDefinition.DeviceId}' is not a valid integer");
            }

            return new AndroidUsbDevice(UsbManager, Context, deviceId, 3000);
        }
        #endregion

        #region Public Static Methods
        public static DeviceDefinitionPlus GetAndroidDeviceDefinition(UsbDevice usbDevice)
        {
            var deviceId = usbDevice.DeviceId.ToString();
            Logger.Log($"Found device: {usbDevice.ProductName} Id: {deviceId}", null, nameof(AndroidUsbDeviceFactory));

            return new DeviceDefinitionPlus(deviceId)
            {
                ProductName = usbDevice.ProductName,
                Manufacturer = usbDevice.ManufacturerName,
                SerialNumber = usbDevice.SerialNumber,
                ProductId = (uint)usbDevice.ProductId,
                VendorId = (uint)usbDevice.VendorId,
                DeviceType = DeviceType.Usb
            };
        }

        public static void Register(UsbManager usbManager, Context context)
        {
            DeviceManager.Current.DeviceFactories.Add(new AndroidUsbDeviceFactory(usbManager, context));
        }
        #endregion
    }
}
