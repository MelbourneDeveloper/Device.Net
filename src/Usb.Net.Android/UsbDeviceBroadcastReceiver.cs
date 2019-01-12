using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using System;

namespace Usb.Net.Android
{
    public class UsbDeviceBroadcastReceiver : BroadcastReceiver
    {
        #region Fields
        private readonly DeviceListener _DeviceListener;
        #endregion

        #region Constructor
        public UsbDeviceBroadcastReceiver(DeviceListener deviceListener)
        {
            _DeviceListener = deviceListener;
        }
        #endregion

        #region Overrides
        public override async void OnReceive(Context context, Intent intent)
        {
            //No need to get the device because we're going to enumerate them anyway...
            //var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

            await _DeviceListener.CheckForDevicesAsync();

            Logger.Log("Device connected", null, AndroidUsbDevice.LogSection);
        }
        #endregion
    }
}