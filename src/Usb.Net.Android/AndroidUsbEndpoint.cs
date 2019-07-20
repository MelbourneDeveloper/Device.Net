using Android.Hardware.Usb;

namespace Usb.Net.Android
{
    public class AndroidUsbEndpoint : IUsbInterfaceEndpoint
    {
        public UsbEndpoint UsbEndpoint { get; }
        public bool IsRead { get; }
        public bool IsWrite { get; }
        public bool IsInterrupt { get; }
        public byte PipeId { get; }
        public ushort MaxPacketSize => (ushort)UsbEndpoint.MaxPacketSize;

        public AndroidUsbEndpoint(UsbEndpoint usbEndpoint, bool isRead, bool isWrite, bool isInterrupt, byte pipeId)
        {
            IsRead = isRead;
            IsWrite = isWrite;
            IsInterrupt = isInterrupt;
            UsbEndpoint = usbEndpoint;
            PipeId = pipeId;
        }
    }
}