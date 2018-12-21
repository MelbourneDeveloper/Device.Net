using System.Runtime.InteropServices;

namespace Hid.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HidAttributes
    {
        public int Size;
        public short VendorId;
        public short ProductId;
        public short VersionNumber;
    }
}