namespace Usb.Net.Windows
{
    public static partial class WinUsbApiCalls
    {
        public enum USBD_PIPE_TYPE
        {
            UsbdPipeTypeControl,
            UsbdPipeTypeIsochronous,
            UsbdPipeTypeBulk,
            UsbdPipeTypeInterrupt
        }
    }
}
