using Android.Content;
using Android.Hardware.Usb;
using Device.Net;

namespace Usb.Net.Android
{
    public class UsbDeviceDetachedReceiver : BroadcastReceiver
    {
        #region Fields
        readonly AndroidUsbDevice _AndroidHidDevice;
        #endregion

        #region Constructor
        public UsbDeviceDetachedReceiver(AndroidUsbDevice androidHidDevice)
        {
            _AndroidHidDevice = androidHidDevice;
        }
        #endregion

        #region Overrides
        public override async void OnReceive(Context context, Intent intent)
        {
            var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

            if (_AndroidHidDevice == null || device == null || device.VendorId != _AndroidHidDevice.VendorId || device.ProductId != _AndroidHidDevice.ProductId) return;

            await _AndroidHidDevice.UsbDeviceDetached();
            Logger.Log("Device detached", null, AndroidUsbDevice.LogSection);
        }
        #endregion
    }
}