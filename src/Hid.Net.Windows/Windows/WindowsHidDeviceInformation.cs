using Device.Net;

namespace Hid.Net.Windows
{
    public class WindowsHidDeviceInformation : DeviceInformation
    {
        public int InputReportByteLength { get; set; }
        public string Manufacturer { get; set; }
        public int OutputReportByteLength { get; set; }
        public string Product { get; set; }
        public ushort ProductId { get; set; }
        public string SerialNumber { get; set; }
        public ushort Usage { get; set; }
        public ushort UsagePage { get; set; }
        public ushort VendorId { get; set; }
        public ushort VersionNumber { get; set; }
    }
}
