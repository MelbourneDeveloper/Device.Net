using System.Runtime.InteropServices;

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WINUSB_SETUP_PACKET
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public byte RequestType;
        public byte Request;
        public ushort Value;
        public ushort Index;
        public ushort Length;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}
