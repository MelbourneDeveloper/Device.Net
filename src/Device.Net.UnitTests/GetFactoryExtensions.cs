#if !NET45


#if !WINDOWS_UWP
using Hid.Net.Windows;
using Usb.Net.Windows;
#else
using Usb.Net.UWP;
using Hid.Net.UWP;
#endif

namespace Device.Net.UnitTests
{
    public static class GetFactoryExtensions
    {
        public static IDeviceFactory GetUsbDeviceFactory(this FilterDeviceDefinition filterDeviceDefinition) =>
#if !WINDOWS_UWP
            filterDeviceDefinition.CreateWindowsUsbDeviceFactory();
#else
            filterDeviceDefinition.CreateUwpUsbDeviceFactory();
#endif


        public static IDeviceFactory GetHidDeviceFactory(this FilterDeviceDefinition filterDeviceDefinition, byte? defultReportId = null) =>
#if !WINDOWS_UWP
            filterDeviceDefinition.CreateWindowsHidDeviceFactory(defaultReportId: defultReportId);
#else
            filterDeviceDefinition.CreateUwpHidDeviceFactory(defaultReportId: defultReportId);
#endif
    }
}

#endif
