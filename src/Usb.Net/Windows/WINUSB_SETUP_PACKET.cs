using System.Runtime.InteropServices;

namespace Usb.Net.Windows
{
    public static partial class WinUsbApiCalls
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WINUSB_SETUP_PACKET
        {
            public byte RequestType;
            public byte Request;
            public ushort Value;
            public ushort Index;
            public ushort Length;
        }
    }
}
