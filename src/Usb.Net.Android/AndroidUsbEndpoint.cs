using Android.Hardware.Usb;

namespace Usb.Net.Android
{
    public class AndroidUsbEndpoint : IUsbInterfaceEndpoint
    {
        public UsbEndpoint UsbEndpoint { get; }
        public bool IsRead { get; }
        public bool IsWrite { get; }
        public byte PipeId { get; }
        public ushort WriteBufferSize => (ushort)UsbEndpoint.MaxPacketSize;
        public ushort ReadBufferSize => (ushort)UsbEndpoint.MaxPacketSize;

        public AndroidUsbEndpoint(UsbEndpoint usbEndpoint, bool isRead, bool isWrite, byte pipeId)
        {
            IsRead = isRead;
            IsWrite = isWrite;
            UsbEndpoint = usbEndpoint;
            PipeId = pipeId;
        }
    }
}