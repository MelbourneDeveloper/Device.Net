using Android.Hardware.Usb;

namespace Usb.Net.Android
{
    internal class AndroidUsbInterface : Usb.Net., IUsbInterface
    {
        private UsbInterface _UsbInterface;

        public AndroidUsbInterface(UsbInterface usbInterface)
        {
            _UsbInterface = usbInterface;
        }

    }
}