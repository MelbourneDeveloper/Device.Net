using Android.Content;
using Android.Hardware.Usb;
using Device.Net;

namespace Usb.Net.Android
{
    public class UsbDeviceAttachedReceiver : BroadcastReceiver
    {
        #region Fields
        readonly AndroidUsbDevice _AndroidHidDevice;
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
            var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

            if (_AndroidHidDevice == null || device == null || device.VendorId != _AndroidHidDevice.VendorId || device.ProductId != _AndroidHidDevice.ProductId) return;

            await _AndroidHidDevice.UsbDeviceAttached();
            Logger.Log("Device connected", null, AndroidUsbDevice.LogSection);
        }
        #endregion
    }
}