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
        private TrezorExample _DeviceConnectionExample = new TrezorExample();
        #endregion

        #region Constructor
        public MainPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers


        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            TheProgressRing.IsActive = true;

            try
            {
                await _DeviceConnectionExample.InitializeTrezorAsync();
                var readBuffer = _DeviceConnectionExample.WriteAndReadFromDeviceAsync();
                DevicePanel.DataContext = _DeviceConnectionExample.TrezorDevice.DeviceDefinition;
                OutputBox.Text = string.Join(' ', readBuffer);
            }
            catch
            {
            }

            TheProgressRing.IsActive = false;
        }

        private void PollButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonColor(Colors.Red);

        }

        private void SetButtonColor(Color backGroundColor)
        {
            PollButton.Background = new SolidColorBrush(backGroundColor);
        }
        #endregion




    }
}
