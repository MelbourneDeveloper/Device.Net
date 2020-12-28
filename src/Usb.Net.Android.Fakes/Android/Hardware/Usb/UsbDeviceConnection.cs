using System;
using System.Threading.Tasks;

namespace Android.Hardware.Usb
{
#pragma warning disable IDE1006 // Naming Styles
    public interface UsbDeviceConnection : IDisposable
#pragma warning restore IDE1006 // Naming Styles
    {
        void Close();
        bool ClaimInterface(UsbInterface usbInterface, bool force);
        Task<UsbRequest?> RequestWaitAsync();
        Task<UsbRequest?> RequestWaitAsync(long timeout);
        Task<int> ControlTransferAsync(UsbAddressing requestType, int request, int value, int index, byte[]? buffer, int length, int timeout);
    }
}
