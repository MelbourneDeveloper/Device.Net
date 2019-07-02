using System.Runtime.InteropServices;

namespace Hid.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HidAttributes
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public int Size;
        public short VendorId;
        public short ProductId;
        public short VersionNumber;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}