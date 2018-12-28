namespace Usb.Net.Windows
{
    internal class UsbInterfacePipe
    {
        public WinUsbApiCalls.WINUSB_PIPE_INFORMATION WINUSB_PIPE_INFORMATION { get; set; }
        public bool IsRead => (WINUSB_PIPE_INFORMATION.PipeId & WinUsbApiCalls.WritePipeId) != 0;
        public bool IsWrite => (WINUSB_PIPE_INFORMATION.PipeId & WinUsbApiCalls.WritePipeId) == 0;
    }
}
