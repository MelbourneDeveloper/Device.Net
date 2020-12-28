using System;

namespace Usb.Net.Android
{
    public interface IUsbPermissionBroadcastReceiver
    {
        bool? IsPermissionGranted { get; }
        event EventHandler Received;
        void Register();
    }
}