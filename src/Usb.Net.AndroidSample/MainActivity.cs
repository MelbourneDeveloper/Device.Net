using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.AndroidSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

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

        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
            var view = (View)sender;

            await Go(view);

        }

        private async Task Go(View view)
        {
            try
            {
                //Register the factory for creating Usb devices. This only needs to be done once.
                AndroidUsbDeviceFactory.Register();

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
                using (var trezorDevice = devices.FirstOrDefault())
                {
                    await trezorDevice.InitializeAsync();

                    //Create a buffer with 3 bytes (initialize)
                    var buffer = new byte[64];
                    buffer[0] = 0x3f;
                    buffer[1] = 0x23;
                    buffer[2] = 0x23;

                    //Write the data to the device and get the response
                    var readBuffer = await trezorDevice.WriteAndReadAsync(buffer);

                    if (readBuffer != null && readBuffer.Length > 0)
                    {
                        DisplayMessage(view, $"All good. First three bytes {readBuffer[0]}, {readBuffer[1]}, {readBuffer[2]}");
                    }
                    else
                    {
                        DisplayMessage(view, $"No good. No data returned.");
                    }

                }
            }
            catch (Exception ex)
            {
                DisplayMessage(view, $"No good: {ex.Message}");
            }
        }

        private static void DisplayMessage(View view, string message)
        {
            Snackbar.Make(view, message, Snackbar.LengthLong).SetAction("Action", (View.IOnClickListener)null).Show();
        }
    }
}

