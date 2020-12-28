using Android.Hardware.Usb;
using Android.Content;

namespace Usb.Net.Android
{
    public interface IAndroidFactory
    {
        UsbRequest CreateUsbRequest();
        IntentFilter CreateIntentFilter(string? action);
        Intent CreateIntent(string? action);
    }
}