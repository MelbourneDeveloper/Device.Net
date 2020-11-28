
using Usb.Net.UWP;
using Hid.Net.UWP;
using Microsoft.Extensions.Logging;

namespace Device.Net.UnitTests
{
    public static class GetFactoryExtensions
    {
        public static IDeviceFactory GetUsbDeviceFactory(this FilterDeviceDefinition filterDeviceDefinition, ILoggerFactory loggerFactory) =>

            filterDeviceDefinition.CreateUwpUsbDeviceFactory(loggerFactory);

        public static IDeviceFactory GetHidDeviceFactory(this FilterDeviceDefinition filterDeviceDefinition, ILoggerFactory loggerFactory, byte? defultReportId = null) =>
            filterDeviceDefinition.CreateUwpHidDeviceFactory(loggerFactory, defaultReportId: defultReportId);

    }
}

