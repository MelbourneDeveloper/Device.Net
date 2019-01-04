using Android.Content;
using System;

namespace Usb.Net.Android
{
    public class UsbDeviceAttachedReceiver : BroadcastReceiver
    {
        #region Fields
        private readonly AndroidUsbDevice _AndroidHidDevice;
        #endregion

        #region Constructor
        public UsbDeviceAttachedReceiver(AndroidUsbDevice androidHidDevice)
        {
            _AndroidHidDevice = androidHidDevice;
        }
        #endregion

        #region Overrides
        public override async void OnReceive(Context context, Intent intent)
        {
            throw new NotImplementedException();
            //var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

            //if (_AndroidHidDevice == null || device == null || device.VendorId != _AndroidHidDevice.DeviceDefinition.VendorId || device.ProductId != _AndroidHidDevice.DeviceDefinition.ProductId) return;

            //await _AndroidHidDevice.UsbDeviceAttached();
            //Logger.Log("Device connected", null, AndroidUsbDevice.LogSection);
        }
        #endregion
    }
}