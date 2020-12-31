using Android.Content;
using Android.Hardware.Usb;

namespace Usb.Net.Android
{
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public interface IAndroidFactory
    {
        UsbRequest CreateUsbRequest();
        IntentFilter CreateIntentFilter(string? action);
        Intent CreateIntent(string? action);
    }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
}