using Android.App;
using Android.OS;
using Android.Views;
using Android.Runtime;
using Android.Content.PM;
using Flutnet.Interop.Embedding.Android;
using Flutnet.Interop.Embedding.Engine;
using Android.Hardware.Usb;
using Flutnet;
using FlutnetThermometer.Services;

namespace FlutnetThermometer
{
    [
        Activity(Label = "@string/app_name", Theme = "@style/LaunchTheme", MainLauncher = true,
            // FLUTTER ACTIVITY SETUP
            HardwareAccelerated = true,
            WindowSoftInputMode = SoftInput.AdjustResize,
            ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.Keyboard |
                                   ConfigChanges.ScreenSize | ConfigChanges.Locale |
                                   ConfigChanges.LayoutDirection | ConfigChanges.FontScale | ConfigChanges.ScreenLayout |
                                   ConfigChanges.Density | ConfigChanges.UiMode
        )
    ]
    [MetaData("io.flutter.embedding.android.NormalTheme", Resource = "@style/AppTheme")]
    [MetaData("io.flutter.embedding.android.SplashScreenDrawable", Resource = "@drawable/launch_background")]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : FlutterActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // NOTE: Flutnet environment MUST be initialized BEFORE base.OnCreate
            App.Init(this);
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void ConfigureFlutterEngine(FlutterEngine flutterEngine)
        {
            base.ConfigureFlutterEngine(flutterEngine);

            FlutnetRuntime.RegisterPlatformService(new ThermometerServiceDroid((UsbManager)GetSystemService(UsbService), Context), "thermometer_service");

            // Connect Flutter plugins (uncomment only if Flutter module uses plugins)
            //Flutnet.Interop.Plugins.GeneratedPluginRegistrant.RegisterWith(flutterEngine);

            if (App.Initialized)
                App.ConfigureFlutnetBridge(this.FlutterEngine);
        }

        public override void CleanUpFlutterEngine(FlutterEngine flutterEngine)
        {
            base.CleanUpFlutterEngine(flutterEngine);

            if (App.Initialized)
                App.CleanUpFlutnetBridge();
        }
    }
}