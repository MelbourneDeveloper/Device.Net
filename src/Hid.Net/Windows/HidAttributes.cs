using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Hid.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CA1815 // Override equals and operator equals on value types
    internal struct HidAttributes
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public int Size;
        public short VendorId;
        public short ProductId;
        public short VersionNumber;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}