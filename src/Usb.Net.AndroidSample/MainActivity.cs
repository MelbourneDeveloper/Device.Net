using Android.App;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Device.Net;
using Microsoft.Extensions.Logging;
using System;
using Usb.Net.Android;
using Usb.Net.Sample;

namespace Usb.Net.AndroidSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : AppCompatActivity
    {
        #region Fields
        private TrezorExample _TrezorExample;
        #endregion

        #region Constructor
        public MainActivity()
        {
        }
        #endregion

        #region Protected Override Methods
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.activity_main);

                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                SetSupportActionBar(toolbar);

                var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
                fab.Click += FabOnClick;
            }
            catch (Exception ex)
            {
                DisplayMessage($"Error Starting up: {ex.Message}");
            }
        }
        #endregion

        #region Public Override Methods
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            return id == Resource.Id.action_settings || base.OnOptionsItemSelected(item);
        }
        #endregion

        #region Event Handlers
        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!(GetSystemService(UsbService) is UsbManager usbManager)) throw new Exception("UsbManager is null");

                var loggerFactory = LoggerFactory.Create((builder) => builder.AddDebug());

                var deviceManager = TrezorExample.UsbDeviceDefinitions
                    .CreateAndroidUsbDeviceFactory(usbManager, base.ApplicationContext, loggerFactory: loggerFactory)
                    .ToDeviceManager(loggerFactory);

                _TrezorExample = new TrezorExample(deviceManager, loggerFactory);

                _TrezorExample.TrezorDisconnected += TrezorExample_TrezorDisconnected;
                _TrezorExample.TrezorInitialized += TrezorExample_TrezorInitialized;
                _TrezorExample.StartListening();

                DisplayMessage("Waiting for device...");
            }
            catch (Exception ex)
            {
                DisplayMessage("Failed to start listener..." + ex.Message);
            }
        }

        private async void TrezorExample_TrezorInitialized(object sender, EventArgs e)
        {
            try
            {
                var readBuffer = await _TrezorExample.WriteAndReadFromDeviceAsync();

                if (readBuffer != null && readBuffer.Length > 0)
                {
                    DisplayMessage($"All good. First three bytes {readBuffer[0]}, {readBuffer[1]}, {readBuffer[2]}");
                }
                else
                {
                    DisplayMessage("No good. No data returned.");
                }
            }
            catch (Exception ex)
            {
                DisplayMessage($"No good: {ex.Message}");
            }
        }

        private void TrezorExample_TrezorDisconnected(object sender, EventArgs e) => DisplayMessage("Device disconnected. Waiting for device...");
        #endregion

        #region Private Methods
        private void DisplayMessage(string message)
        {
            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            Snackbar.Make(fab, message, Snackbar.LengthLong).SetAction("Action", (View.IOnClickListener)null).Show();
        }
        #endregion
    }
}

