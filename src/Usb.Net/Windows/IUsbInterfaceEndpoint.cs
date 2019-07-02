namespace Usb.Net.Windows
{
    public interface IUsbInterfaceEndpoint
    {
        bool IsRead { get; }
        bool IsWrite { get; }
        byte PipeId { get; }
    }
}