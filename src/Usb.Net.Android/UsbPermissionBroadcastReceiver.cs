using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using System;

namespace Usb.Net.Android
{
    public class UsbPermissionBroadcastReceiver : BroadcastReceiver
    {
        #region Fields
        /// <summary>
        /// Just used as a filter for the UsbPermissionBroadcastReceiver
        /// </summary>
        private const string RequestUsbIntentAction = nameof(RequestUsbIntentAction);
        private bool? _IsPermissionGranted;
        private UsbManager _Manager;
        private UsbDevice _Device;
        private Context _Context;
        #endregion

        #region Public Properties
        public bool? IsPermissionGranted => _IsPermissionGranted;
        #endregion

        #region Events
        public event EventHandler Received;
        #endregion

        #region Constructor
        public UsbPermissionBroadcastReceiver(UsbManager manager, UsbDevice device, Context context)
        {
            _Manager = manager;
            _Device = device;
            _Context = context;
        }
        #endregion

        #region Public Methods
        public void Register()
        {
            _Context.RegisterReceiver(this, new IntentFilter(RequestUsbIntentAction));
            var pendingIntent = PendingIntent.GetBroadcast(_Context, 0, new Intent(RequestUsbIntentAction), 0);
            _Manager.RequestPermission(_Device, pendingIntent);
        }
        #endregion

        #region Overrides 
        public override void OnReceive(Context context, Intent intent)
        {
            _IsPermissionGranted = intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false);

            Logger.Log($"USB permission broadcast received. Result: {_IsPermissionGranted}", null, AndroidUsbDevice.LogSection);

            context.UnregisterReceiver(this);
            Received?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}