#if WINDOWS_UWP
using Hid.Net.UWP;
#else
using Android.Content;
using Android.Hardware.Usb;
using Usb.Net.Android;
#endif
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Device.Net.UnoSample
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
            Loaded += MainPage_Loaded1;
        }

        private void MainPage_Loaded1(object sender, global::Windows.UI.Xaml.RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded1;

            var loggerFactory = LoggerFactory.Create((builder) => _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace));

            var filterDeviceDefinitions = new List<FilterDeviceDefinition> { new FilterDeviceDefinition(vendorId: 16701, productId: 8455, usagePage: 65280) };

            var deviceDataStreamer =
            filterDeviceDefinitions
#if WINDOWS_UWP
                .CreateUwpHidDeviceFactory(loggerFactory)
#else
                .CreateAndroidUsbDeviceFactory(UsbManager, AppContext, loggerFactory, writeBufferSize: 8, readBufferSize: 8)
#endif
                .CreateDeviceDataStreamer(async (device) =>
                {
                    string display = null;

                    try
                    {
                        //Note this needs an extra 0 at the start for UWP
                        var data = await device.WriteAndReadAsync(new byte[8] { 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 });

                        //Note this data will have an extra 0 at the start on UWP
                        var temperatureTimesOneHundred = (data.Data[3] & 0xFF) + (data.Data[2] << 8);

                        var temperatureCelsius = Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven);

                        display = temperatureCelsius.ToString() + "°C";
                    }
                    catch
                    {
                        display = "Bad read";
                    }

                    if (display == null) return;

                    _ = DispatchingExtensions.RunOnDispatcher(() => TheTextBlock.Text = display);

                }
                //Note this breaks UWP
                , async (d) =>
                {
                    await d.InitializeAsync();
                }
                ).Start();
        }
    }
}
