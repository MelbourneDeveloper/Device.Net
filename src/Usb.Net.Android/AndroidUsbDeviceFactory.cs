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
        public DeviceType DeviceType => throw new NotImplementedException();
        #endregion

        #region Constructor
        public AndroidUsbDeviceFactory(UsbManager usbManager, Context context)
        {
            UsbManager = usbManager;
            Context = context;
        }
        #endregion

        #region Public Methods
        public Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            return Task.Run<IEnumerable<DeviceDefinition>>(() => 
            {
                //TODO: Get more details about the device.
                return UsbManager.DeviceList.Select(kvp => kvp.Value).Select(d => new DeviceDefinition { ProductName = d.ProductName, Manufacturer = d.ManufacturerName, SerialNumber = d.SerialNumber, DeviceId = d.DeviceId.ToString(), ProductId = (uint)d.ProductId, VendorId = (uint)d.VendorId, DeviceType = DeviceType.Usb }).ToList();
            });
        }

        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            if (!int.TryParse(deviceDefinition.DeviceId, out var deviceId))
            {
                throw new Exception($"The device Id {deviceDefinition.DeviceId} is not a valid integer");
            }

            return new AndroidUsbDevice(UsbManager, Context, deviceId, 3000);
        }
        #endregion

        #region Public Static Methods
        public static void Register(UsbManager usbManager, Context context)
        {
            DeviceManager.Current.DeviceFactories.Add(new AndroidUsbDeviceFactory(usbManager, context));
        }
        #endregion
    }
}
