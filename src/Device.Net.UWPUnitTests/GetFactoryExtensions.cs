
using Hid.Net;
using Hid.Net.UWP;
using Microsoft.Extensions.Logging;
using System;
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
#pragma warning disable IDE0060 // Remove unused parameter
            ILoggerFactory loggerFactory, object classGuid = null)
#pragma warning restore IDE0060 // Remove unused parameter
            => filterDeviceDefinition.CreateUwpUsbDeviceFactory(loggerFactory);

        public static IDeviceFactory GetHidDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory,
            Func<Report, TransferResult> readReportTransform = null,
            Func<byte[], byte, byte[]> writeTransferTransform = null)
            => filterDeviceDefinition.CreateUwpHidDeviceFactory(
                loggerFactory,
                readReportTransform: readReportTransform,
                writeTransferTransform: writeTransferTransform);
    }
}

