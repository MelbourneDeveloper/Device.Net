using Device.Net;
using Hid.Net.UWP;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


            await InitializeTrezor();
        }

        private static async Task InitializeTrezor()
        {
            var windowsDevice = new UWPUsbDevice(null) { VendorId = 0x1209, ProductId = 0x53c1 };
            var devicePoller = new DevicePoller(new List<IDevice> { windowsDevice }, 3000);
            devicePoller.DeviceDisconnected += DevicePoller_DeviceDisconnected;
            devicePoller.DeviceInitialized += DevicePoller_DeviceInitialized;

        }

        private static void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("init");
        }

        private static void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("disconnected");
        }
    }
}
