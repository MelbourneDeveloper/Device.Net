using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using usbDevice = Android.Hardware.Usb.UsbDevice;

namespace Usb.Net.Android
{
    /// <summary>
    /// 
    /// </summary>
    public class UsbPermissionBroadcastReceiver : BroadcastReceiver, IUsbPermissionBroadcastReceiver
    {
        #region Fields
        /// <summary>
        /// Just used as a filter for the UsbPermissionBroadcastReceiver
        /// </summary>
        private const string RequestUsbIntentAction = nameof(RequestUsbIntentAction);
        private readonly UsbManager _Manager;
        private readonly usbDevice _Device;
        private readonly Context _Context;
        private readonly ILogger _logger;
        private readonly IAndroidFactory _androidFactory;
        #endregion

        #region Public Properties
        public bool? IsPermissionGranted { get; private set; }
        #endregion

        #region Events
        public event EventHandler Received;
        #endregion

        #region Constructor
        public UsbPermissionBroadcastReceiver(
            UsbManager manager,
            usbDevice device,
            Context context,
            IAndroidFactory androidFactory,
            ILogger logger = null)
        {
            _Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _Device = device ?? throw new ArgumentNullException(nameof(device));
            _Context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? NullLogger.Instance;
            _androidFactory = androidFactory;
        }
        #endregion

        #region Public Methods
        public void Register()
        {
            _ = _Context.RegisterReceiver(this, _androidFactory.CreateIntentFilter(RequestUsbIntentAction));
            _logger.LogInformation("Receiver registered", IsPermissionGranted, _Device.DeviceId);
            var pendingIntent = PendingIntent.GetBroadcast(_Context, 0, _androidFactory.CreateIntent(RequestUsbIntentAction), 0);
            _Manager.RequestPermission(_Device, pendingIntent);
            _logger.LogInformation("Permission requested", IsPermissionGranted, _Device.DeviceId);
        }
        #endregion

        #region Overrides 
        public override void OnReceive(Context context, Intent intent)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (intent == null) throw new ArgumentNullException(nameof(intent));

            IsPermissionGranted = intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false);

            _logger.LogInformation("USB permission broadcast received. Result: {IsPermissionGranted} DeviceId: {deviceId}", IsPermissionGranted, _Device.DeviceId);

            context.UnregisterReceiver(this);
            Received?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}