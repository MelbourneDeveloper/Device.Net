namespace Usb.Net
{
    public interface IUsbInterfaceEndpoint
    {
        bool IsRead { get; }
        bool IsWrite { get; }
        bool IsInterrupt { get; }
        byte PipeId { get; }

        //TODO: Remove one of these and change it to just BufferSize
        ushort ReadBufferSize { get; }
        ushort WriteBufferSize { get; }
    }
}