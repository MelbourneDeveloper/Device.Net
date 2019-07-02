namespace Usb.Net
{
    public interface IUsbInterfaceEndpoint
    {
        bool IsRead { get; }
        bool IsWrite { get; }
        byte PipeId { get; }
    }
}