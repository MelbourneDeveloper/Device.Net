namespace Usb.Net
{
    public interface IUsbInterfaceEndpoint
    {
        bool IsRead { get; }
        bool IsWrite { get; }
        bool IsInterrupt { get; }
        byte PipeId { get; }
        ushort MaxPacketSize { get; }
    }
}