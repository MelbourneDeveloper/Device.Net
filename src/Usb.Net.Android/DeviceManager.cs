using Android.Content;
using Android.Hardware.Usb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net.Android;

namespace Device.Net
{
    public class AndroidDeviceFactoryBase : IDeviceFactory
    {
        public UsbManager UsbManager { get; }
        public Context Context { get; }

        #region Public Static Properties
        public static DeviceManager Current { get; set; }

        public DeviceType DeviceType => throw new NotImplementedException();
        #endregion

        #region Constructor
        public AndroidDeviceFactoryBase(UsbManager usbManager, Context context)
        {
            UsbManager = usbManager;
            Context = context;
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            //TODO: Get the values necessary to construct the device.
            return UsbManager.DeviceList.Select(kvp => kvp.Value).Select(d => new DeviceDefinition { DeviceId = d.DeviceId.ToString(), ProductId = (uint)d.ProductId, VendorId = (uint)d.VendorId, DeviceType = DeviceType.Usb }).ToList();
        }

        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return new AndroidUsbDevice(UsbManager, Context, 3000, deviceDefinition.ReadBufferSize.Value, (int)deviceDefinition.VendorId.Value, (int)deviceDefinition.ProductId.Value);
        }
        #endregion
    }
}
