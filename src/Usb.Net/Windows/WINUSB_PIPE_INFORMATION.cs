using System.Runtime.InteropServices;

namespace Usb.Net.Windows
{
    public static partial class WinUsbApiCalls
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct WINUSB_PIPE_INFORMATION
        {
            public USBD_PIPE_TYPE PipeType;
            public byte PipeId;
            public ushort MaximumPacketSize;
            public byte Interval;
        }
    }
}
