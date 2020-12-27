

using System.Collections.Generic;

namespace Android.Hardware.Usb
{
    public interface UsbManager
    {
        public const string ExtraPermissionGranted = "permission";
        IDictionary<string, UsbDevice>? DeviceList { get; }
        UsbDeviceConnection OpenDevice(UsbDevice usbDevice);
        public virtual void RequestPermission(UsbDevice? device, PendingIntent? pi);
    }
}
