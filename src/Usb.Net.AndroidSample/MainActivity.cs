using Android.App;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Device.Net;
using System;
using Android.Content;
using AndroidSampleCamera;
using AndroidUsb = Android.Hardware.Usb;

namespace Usb.Net.AndroidSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region Fields
        //private readonly TrezorExample _TrezorExample = new TrezorExample();
        private readonly CameraExample _cameraExample = new CameraExample();
        private static string ACTION_USB_PERMISSION = "com.android.example.USB_PERMISSION";
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
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        #endregion

        #region Event Handlers
        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var usbManager = GetSystemService(UsbService) as UsbManager;
                if (usbManager == null) throw new Exception("UsbManager is null");

                //Register the factory for creating Usb devices. This only needs to be done once.
                AndroidUsbDeviceFactory.Register(usbManager, base.ApplicationContext);

                //_TrezorExample.TrezorDisconnected += _TrezorExample_TrezorDisconnected;
                //_TrezorExample.TrezorInitialized += _TrezorExample_TrezorInitialized;
                //_TrezorExample.StartListening();

                _cameraExample.CameraInitialized += CameraExampleOnCameraInitialized;
                _cameraExample.CameraDisconnected += CameraExampleOnCameraDisconnected;
                _cameraExample.Error += CameraExampleOnError;

                TestUsbReciever usbReciever = new TestUsbReciever(_cameraExample);
                usbReciever.OnError += (o, s) => DisplayMessage($"Error in Receiver: {s}");
                var mPermissionIntent = PendingIntent.GetBroadcast(this, 0, new Intent(ACTION_USB_PERMISSION), 0);
                var filter = new IntentFilter(ACTION_USB_PERMISSION);
                RegisterReceiver(usbReciever, filter);

                AndroidUsb.UsbDevice myDevice = null;
                foreach (var dev in usbManager.DeviceList)
                {
                    DisplayMessage($"V: {dev.Value.VendorId}");
                    if (dev.Value.VendorId == 1200)
                    {
                        myDevice = dev.Value;
                    }
                }

                usbManager.RequestPermission(myDevice, mPermissionIntent);
                bool hasPermission = usbManager.HasPermission(myDevice);
                //DisplayMessage($"Permission Granted: {hasPermission}");
                //if (hasPermission)
                //{
                //    var androidCamera = new AndroidUsbDevice(usbManager, base.ApplicationContext, myDevice.DeviceId, new DebugLogger());
                //    await _cameraExample.InitializeForDeviceAsync(androidCamera);
                //}

                //TODO: Why isn't this working?
                //_cameraExample.StartListening();

                //var attachedReceiver = new UsbDeviceBroadcastReceiver(_TrezorExample.DeviceListener);
                //var detachedReceiver = new UsbDeviceBroadcastReceiver(_TrezorExample.DeviceListener);
                //RegisterReceiver(attachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));
                //RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));


                //DisplayMessage("Waiting for device...");
            }
            catch(Exception ex)
            {
                DisplayMessage("Failed to start listener..." + ex.Message);
            }
        }

        private void CameraExampleOnError(object sender, string e)
        {
            DisplayMessage($"Camera Error: {e}");
        }

        private void CameraExampleOnCameraDisconnected(object sender, EventArgs e)
        {
            DisplayMessage("Device disconnected. Waiting for device..."); 
        }

        private async void CameraExampleOnCameraInitialized(object sender, EventArgs e)
        {
            try
            {
                await _cameraExample.OpenSession();
                var deviceInfo = await _cameraExample.GetDeviceInfo();
                if(deviceInfo.Manufacturer == null)
                    throw new ApplicationException($"Unable to get Device Info, it is null.");

                DisplayMessage($"{deviceInfo.Manufacturer} - {deviceInfo.Model} Connected.");
            }
            catch (Exception ex)
            {
                DisplayMessage($"No good: {ex.Message}");
            }
        }

        private async void _TrezorExample_TrezorInitialized(object sender, EventArgs e)
        {
            //try
            //{
            //    var readBuffer = await _TrezorExample.WriteAndReadFromDeviceAsync();

            //    if (readBuffer != null && readBuffer.Length > 0)
            //    {
            //        DisplayMessage($"All good. First three bytes {readBuffer[0]}, {readBuffer[1]}, {readBuffer[2]}");
            //    }
            //    else
            //    {
            //        DisplayMessage($"No good. No data returned.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    DisplayMessage($"No good: {ex.Message}");
            //}
        }

        private void _TrezorExample_TrezorDisconnected(object sender, EventArgs e)
        {
            DisplayMessage("Device disconnected. Waiting for device...");
        }
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

