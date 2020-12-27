

using Android.App;
using System.Collections.Generic;

namespace Android.Hardware.Usb
{
    public interface UsbManager
    {
        public const string ExtraPermissionGranted = "permission";
        IDictionary<string, UsbDevice>? DeviceList { get; }
        UsbDeviceConnection OpenDevice(UsbDevice usbDevice);
        void RequestPermission(UsbDevice? device, PendingIntent? pi);
    }
}
