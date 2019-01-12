using Hid.Net.UWP;
using System.Threading.Tasks;
using Usb.Net.Sample;
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
        private TrezorExample _DeviceConnectionExample = new TrezorExample();
        #endregion

        #region Constructor
        public MainPage()
        {
            InitializeComponent();
            _DeviceConnectionExample.TrezorInitialized += _DeviceConnectionExample_TrezorInitialized;
            _DeviceConnectionExample.TrezorDisconnected += _DeviceConnectionExample_TrezorDisconnected;

            //Register the factory for creating Usb devices. This only needs to be done once.
            UWPUsbDeviceFactory.Register();

            //Register the factory for creating Usb devices. This only needs to be done once.
            UWPHidDeviceFactory.Register();

        }
        #endregion

        #region Event Handlers
        private void _DeviceConnectionExample_TrezorDisconnected(object sender, System.EventArgs e)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetButtonColor(Colors.Red);
                OutputBox.Text = string.Empty;
                DevicePanel.DataContext = null;
            });
        }

        private async void _DeviceConnectionExample_TrezorInitialized(object sender, System.EventArgs e)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetButtonColor(Colors.Green);
                DisplayDataAsync(false);
            });
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            await DisplayDataAsync(true);
        }

        private void StartListeningButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonColor(Colors.Red);
            _DeviceConnectionExample.StartListening();
        }
        #endregion

        #region Private Methods
        private async Task DisplayDataAsync(bool initialize)
        {
            TheProgressRing.IsActive = true;

            try
            {
                if (initialize) await _DeviceConnectionExample.InitializeTrezorAsync();
                var readBuffer = await _DeviceConnectionExample.WriteAndReadFromDeviceAsync();
                DevicePanel.DataContext = _DeviceConnectionExample.TrezorDevice.DeviceDefinition;
                OutputBox.Text = string.Join(' ', readBuffer);
            }
            catch
            {
            }

            TheProgressRing.IsActive = false;
        }

        private void SetButtonColor(Color backGroundColor)
        {
            PollButton.Background = new SolidColorBrush(backGroundColor);
        }
        #endregion
    }
}
