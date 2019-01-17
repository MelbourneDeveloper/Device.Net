namespace Usb.Net.Windows
{
    public class UsbInterfacePipe
    {
        public byte PipeId { get; }

        internal UsbInterfacePipe(byte pipeId)
        {
            PipeId = pipeId;
        }

        public bool IsRead => (PipeId & WinUsbApiCalls.WritePipeId) != 0;
        public bool IsWrite => (PipeId & WinUsbApiCalls.WritePipeId) == 0;
    }
}
