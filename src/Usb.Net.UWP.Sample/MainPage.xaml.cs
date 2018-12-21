using Device.Net;
using Hid.Net.UWP;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Usb.Net.UWP.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;

            UWPUsbDeviceFactory.Register();
            UWPHidDeviceFactory.Register();
            var devices = await DeviceManager.Current.GetDevices(new List<DeviceDefinition> { new DeviceDefinition { VendorId = 0x1209, ProductId = 0x53C1 } });
            var trezorDevice = devices.FirstOrDefault();
            await trezorDevice.InitializeAsync();

            var buffer = new byte[64];
            buffer[0] = 0x3f;
            buffer[1] = 0x23;
            buffer[2] = 0x23;

            await trezorDevice.WriteAsync(buffer);

            var readBuffer = await trezorDevice.ReadAsync();
        }
    }
}
