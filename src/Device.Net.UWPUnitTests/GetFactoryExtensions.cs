
using Hid.Net.UWP;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Usb.Net.UWP;

namespace Device.Net.UnitTests
{
    public static class GetFactoryExtensions
    {
        public static IDeviceFactory GetUsbDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory)
            => filterDeviceDefinitions.CreateUwpUsbDeviceFactory(loggerFactory);

        public static IDeviceFactory GetHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory)
            => filterDeviceDefinitions.CreateUwpHidDeviceFactory(loggerFactory);

        public static IDeviceFactory GetUsbDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory)
            => filterDeviceDefinition.CreateUwpUsbDeviceFactory(loggerFactory);

        public static IDeviceFactory GetHidDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory,
            byte? defultReportId = null)
            => filterDeviceDefinition.CreateUwpHidDeviceFactory(loggerFactory, defaultReportId: defultReportId);
    }
}

