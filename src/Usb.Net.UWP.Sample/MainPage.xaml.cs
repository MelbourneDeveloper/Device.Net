using Device.Net;
using Hid.Net.UWP;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Usb.Net.UWP.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields
        //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
        private List<DeviceDefinition> _DeviceDefinitions = new List<DeviceDefinition>
        {
            new DeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x" },
            new DeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C1, ReadBufferSize=64, WriteBufferSize=64, Label="Trezor One Firmware 1.7.x" },
            new DeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C0, ReadBufferSize=64, WriteBufferSize=64, Label="Model T" }
        };

        private IDevice _TrezorDevice;
        #endregion

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

        private async void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            _TrezorDevice = e.Device;
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 SetButtonColor(Colors.Green);
                 WriteAndReadFromDevice();
             });
        }

        private void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            _TrezorDevice = null;

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetButtonColor(Colors.Red);
                OutputBox.Text = string.Empty;
                DevicePanel.DataContext = null;
            });

        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            await InitializeTrezor();
        }

        private void PollButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonColor(Colors.Red);

            _TrezorDevice?.Dispose();

            //Nasty
            //TODO: Remove creating the device... There no need to create the device. It will get created anyway.
            var devicePoller = new DeviceListener(_DeviceDefinitions, 3000);
            devicePoller.DeviceDisconnected += DevicePoller_DeviceDisconnected;
            devicePoller.DeviceInitialized += DevicePoller_DeviceInitialized;
        }

        private void SetButtonColor(Color backGroundColor)
        {
            PollButton.Background = new SolidColorBrush(backGroundColor);
        }
        #endregion

        #region Private Methods
        private async Task WriteAndReadFromDevice()
        {
            TheProgressRing.IsActive = true;

            try
            {
                DevicePanel.DataContext = _TrezorDevice.DeviceDefinition;

                //Create a buffer with 3 bytes (initialize)
                var writeBuffer = new byte[64];
                writeBuffer[0] = 0x3f;
                writeBuffer[1] = 0x23;
                writeBuffer[2] = 0x23;

                //Write the data to the device
                var readBuffer = await _TrezorDevice.WriteAndReadAsync(writeBuffer);
                OutputBox.Text = string.Join(' ', readBuffer);
            }
            catch
            {

            }

            TheProgressRing.IsActive = false;
        }
        #endregion

        #region Private Static Methods
        private async Task InitializeTrezor()
        {
            TheProgressRing.IsActive = true;

            try
            {
                //Get the first available device and connect to it
                var devices = await DeviceManager.Current.GetDevices(_DeviceDefinitions);
                _TrezorDevice = devices.FirstOrDefault();
                await _TrezorDevice.InitializeAsync();
                await WriteAndReadFromDevice();
            }
            catch
            {
            }

            TheProgressRing.IsActive = false;

        }
        #endregion
    }
}
