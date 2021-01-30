using Android.App;
using Android.Views;
using Windows.UI.Xaml;

namespace Device.Net.UnoSample.Droid
{
    [Activity(
    MainLauncher = true,
    ConfigurationChanges = Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden
        )]
    public class MainActivity : ApplicationActivity
    {
    }
}

