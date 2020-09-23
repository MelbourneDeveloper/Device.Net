using Windows.UI.Xaml.Controls;
using System;
using Device.Net;
using Microsoft.Extensions.Logging;
using Device.Net.Reactive;
using System.Collections.Generic;

#if WINDOWS_UWP
using Hid.Net.UWP;
#else
using Android.Hardware.Usb;
using Android.Content;
using Usb.Net.Android;
#endif

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0022 // Use expression body for methods
#pragma warning disable CA1305 // Specify IFormatProvider

namespace UnoCrossPlatform
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
#if !WINDOWS_UWP
        public static UsbManager UsbManager { get; set; }
        public static Context AppContext { get; set; }
#endif

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;

            var loggerFactory = LoggerFactory.Create((builder) =>
            {
                _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
            });

            var filterDeviceDefinitions = new List<FilterDeviceDefinition> { new FilterDeviceDefinition { VendorId = 16701, ProductId = 8455, UsagePage = 65280 } };

            var deviceDataStreamer =
            filterDeviceDefinitions
#if WINDOWS_UWP
                .CreateUwpHidDeviceFactory(loggerFactory)
#else
                .CreateAndroidUsbDeviceFactory(loggerFactory, UsbManager, AppContext, writeBufferSize: 9, readBufferSize: 9)
#endif
                .ToDeviceManager(loggerFactory)
                .CreateDeviceDataStreamer(async (device) =>
                {
                    string display = null;

                    try
                    {
                        var data = await device.WriteAndReadAsync(new byte[9] { 0x00, 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 });

                        var temperatureTimesOneHundred = (data.Data[4] & 0xFF) + (data.Data[3] << 8);

                        var temperatureCelsius = Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven);

                        display = temperatureCelsius.ToString() + "°C";
                    }
                    catch
                    {
                        display = "Bad read";
                    }

                    if (display == null) return;

                    _ = DispatchingExtensions.RunOnDispatcher(() =>
                    {
                        TheTextBlock.Text = display;
                    });

                }).Start();
        }
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scope
#pragma warning restore IDE0022 // Use expression body for methods
#pragma warning restore CA1305 // Specify IFormatProvider
