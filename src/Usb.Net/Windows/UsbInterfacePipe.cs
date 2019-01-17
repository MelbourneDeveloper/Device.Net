namespace Usb.Net.Windows
{
    public class UsbInterfacePipe
    {
        private int PipeId { get; }

        internal UsbInterfacePipe(int pipeId)
        {
            PipeId = pipeId;
        }

        //public WinUsbApiCalls.WINUSB_PIPE_INFORMATION WINUSB_PIPE_INFORMATION { get; set; }
        public bool IsRead => (PipeId & WinUsbApiCalls.WritePipeId) != 0;
        public bool IsWrite => (PipeId & WinUsbApiCalls.WritePipeId) == 0;
    }
}
