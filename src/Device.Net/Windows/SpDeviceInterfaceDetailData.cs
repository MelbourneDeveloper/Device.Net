using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Device.Net.Windows
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SpDeviceInterfaceDetailData
    {
        public int CbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;
    }
}