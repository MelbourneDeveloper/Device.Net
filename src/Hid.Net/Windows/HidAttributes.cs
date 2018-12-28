using System.Runtime.InteropServices;

namespace Hid.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HidAttributes
    {
        public int Size;
        public short VendorId;
        public short ProductId;
        public short VersionNumber;
    }
}