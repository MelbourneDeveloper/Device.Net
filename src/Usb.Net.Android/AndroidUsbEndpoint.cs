using Android.Hardware.Usb;

namespace Usb.Net.Android
{
    public class AndroidUsbEndpoint : IUsbInterfaceEndpoint
    {
        private readonly UsbEndpoint UsbEndpoint;
        public bool IsRead { get; }
        public bool IsWrite { get; }
        public byte PipeId { get; }

        public AndroidUsbEndpoint(UsbEndpoint usbEndpoint, bool isRead, bool isWrite, byte pipeId)
        {
            IsRead = isRead;
            IsWrite = isWrite;
            UsbEndpoint = usbEndpoint;
            PipeId = pipeId;
        }
    }
}