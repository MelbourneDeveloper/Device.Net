using Android.Hardware.Usb;
using Android.Runtime;
using Com.Nostra13.Universalimageloader.Core;
using System;
using Windows.UI.Xaml.Media;

namespace UnoCrossPlatform.Droid
{
    [Android.App.Application(
        Label = "@string/ApplicationName",
        LargeHeap = true,
        HardwareAccelerated = true,
        Theme = "@style/AppTheme"
    )]
    public class Application : Windows.UI.Xaml.NativeApplication
    {
        public Application(IntPtr javaReference, JniHandleOwnership transfer)
            : base(() => new App(), javaReference, transfer) => ConfigureUniversalImageLoader();

        private void ConfigureUniversalImageLoader()
        {
            MainPage.AppContext = Context;
            MainPage.UsbManager = (UsbManager)GetSystemService(UsbService);

            // Create global configuration and initialize ImageLoader with this config
            var config = new ImageLoaderConfiguration
                .Builder(Context)
                .Build();

            ImageLoader.Instance.Init(config);

            ImageSource.DefaultImageLoader = ImageLoader.Instance.LoadImageAsync;
        }
    }
}
