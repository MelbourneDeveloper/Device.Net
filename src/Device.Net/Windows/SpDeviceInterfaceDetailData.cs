using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SpDeviceInterfaceDetailData
    {
        public int CbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;
    }
}