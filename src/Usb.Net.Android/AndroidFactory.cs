using Android.Content;
using Android.Hardware.Usb;

#nullable enable

namespace Usb.Net.Android
{
    public class AndroidFactory : IAndroidFactory
    {
        public Intent CreateIntent(string action) => new Intent(action);
        public IntentFilter CreateIntentFilter(string action) => new IntentFilter(action);
        public UsbRequest CreateUsbRequest() => new UsbRequest();
    }
}