namespace Android.Hardware.Usb
{
    public interface UsbEndpoint
    {
        int EndpointNumber { get; }
        int MaxPacketSize { get; set; }
        UsbAddressing Address { get; }
        int Attributes { get; }
        UsbAddressing Direction { get; }
        UsbAddressing Type { get; }
    }
}