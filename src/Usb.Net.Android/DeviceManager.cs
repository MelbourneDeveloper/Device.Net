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
        #region Public Properties
        public UsbManager UsbManager { get; }
        public Context Context { get; }
        #endregion

        #region Public Static Properties
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
        public Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            //TODO: Get the values necessary to construct the device.
            var deviceDefinitions = UsbManager.DeviceList.Select(kvp => kvp.Value).Select(d => new DeviceDefinition { DeviceId = d.DeviceId.ToString(), ProductId = (uint)d.ProductId, VendorId = (uint)d.VendorId, DeviceType = DeviceType.Usb }).ToList();
            return Task.FromResult<IEnumerable<DeviceDefinition>>(deviceDefinitions);
        }

        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return new AndroidUsbDevice(UsbManager, Context, 3000, deviceDefinition.ReadBufferSize.Value, (int)deviceDefinition.VendorId.Value, (int)deviceDefinition.ProductId.Value);
        }
        #endregion

        #region Public Static Methods
        public static void Register(UsbManager usbManager, Context context)
        {
            DeviceManager.Current.DeviceFactories.Add(new AndroidDeviceFactoryBase(usbManager, context));
        }
        #endregion
    }
}
