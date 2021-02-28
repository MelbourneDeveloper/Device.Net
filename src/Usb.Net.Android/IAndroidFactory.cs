using Android.Content;
using Android.Hardware.Usb;

#nullable enable

namespace Usb.Net.Android
{
    public interface IAndroidFactory
    {
        UsbRequest CreateUsbRequest();
        IntentFilter CreateIntentFilter(string action);
        Intent CreateIntent(string action);
    }
}