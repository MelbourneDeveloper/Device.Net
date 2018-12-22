//using Android.Hardware.Usb;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Device.Net
//{
//    public class DeviceManager
//    {
//        public UsbManager UsbManager { get; }

//        #region Public Static Properties
//        public static DeviceManager Current { get; set; }
//        #endregion

//        #region Constructor
//        public DeviceManager(UsbManager usbManager)
//        {
//            UsbManager = usbManager;
//        }
//        #endregion

//        #region Public Methods
//        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId, DeviceType deviceType)
//        {
//            if (deviceType == DeviceType.Hid) throw new Exception("Android does not support Hid");

//            var devices = UsbManager.DeviceList.Select(kvp => kvp.Value).ToList();

//            //TODO: return the vid/pid if we can get it from the properties. Also read/write buffer size

//            var deviceIds = devices.Select(d => new DeviceDefinition { DeviceId = d.DeviceId.ToString(), ProductId = (uint)d.ProductId, VendorId = (uint)d.VendorId, DeviceType = deviceType }).ToList();

//            return deviceIds;
//        }
//        #endregion
//    }
//}
