using System.Runtime.InteropServices;
using static Usb.Net.Windows.WinUsbApiCalls;

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WINUSB_PIPE_INFORMATION
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public USBD_PIPE_TYPE PipeType;
        public byte PipeId;
        public ushort MaximumPacketSize;
        public byte Interval;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}
