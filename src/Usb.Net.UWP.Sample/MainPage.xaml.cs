using Device.Net;
using Hid.Net.UWP;
using System.Collections.Generic;
using System.Diagnostics;
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
        #region Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }
        #endregion

        #region Event Handlers
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;

            //Register the factory for creating Usb devices. This only needs to be done once.
            UWPUsbDeviceFactory.Register();

            //Register the factory for creating Usb devices. This only needs to be done once.
            UWPHidDeviceFactory.Register();
        }

        private static void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            Debug.WriteLine("init");
        }

        private static void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Debug.WriteLine("disconnected");
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            await InitializeTrezor();
        }

        private void PollButton_Click(object sender, RoutedEventArgs e)
        {
            var windowsDevice = new UWPUsbDevice(null) { VendorId = 0x1209, ProductId = 0x53c1 };
            var devicePoller = new DevicePoller(new List<IDevice> { windowsDevice }, 3000);
            devicePoller.DeviceDisconnected += DevicePoller_DeviceDisconnected;
            devicePoller.DeviceInitialized += DevicePoller_DeviceInitialized;
        }
        #endregion

        #region Private Static Methods
        private static async Task InitializeTrezor()
        {
            //Register the factory for creating Usb devices. This only needs to be done once.
            UWPUsbDeviceFactory.Register();

            //Register the factory for creating Usb devices. This only needs to be done once.
            UWPHidDeviceFactory.Register();

            //Note: other custom device types could be added here

            //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
            var deviceDefinitions = new List<DeviceDefinition>
            {
                new DeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x" },
                new DeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C1, ReadBufferSize=64, WriteBufferSize=64, Label="Trezor One Firmware 1.7.x" },
                new DeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C0, ReadBufferSize=64, WriteBufferSize=64, Label="Model T" }
            };

            //Get the first available device and connect to it
            var devices = await DeviceManager.Current.GetDevices(deviceDefinitions);
            var trezorDevice = devices.FirstOrDefault();
            await trezorDevice.InitializeAsync();

            //Create a buffer with 3 bytes (initialize)
            var writeBuffer = new byte[64];
            writeBuffer[0] = 0x3f;
            writeBuffer[1] = 0x23;
            writeBuffer[2] = 0x23;

            //Write the data to the device
            var readBuffer = await trezorDevice.WriteAndReadAsync(writeBuffer);
        }
        #endregion
    }
}
