using Java.Nio;

namespace Android.Hardware.Usb
{
    public interface UsbRequest
    {
        bool Initialize(UsbDeviceConnection? connection, UsbEndpoint? endpoint);
        bool Queue(ByteBuffer? buffer, int length);
    }
}
