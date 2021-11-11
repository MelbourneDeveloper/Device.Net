
using Hid.Net;
using Hid.Net.Windows;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Usb.Net.Windows;

namespace Device.Net.UnitTests
{
    public static class GetFactoryExtensions
    {
        public static IDeviceFactory GetUsbDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory,
            Guid? classGuid = null)
            => filterDeviceDefinition.CreateWindowsUsbDeviceFactory(loggerFactory, classGuid: classGuid);

        public static IDeviceFactory GetUsbDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory,
            Guid? classGuid = null)
            => filterDeviceDefinitions.CreateWindowsUsbDeviceFactory(loggerFactory, classGuid: classGuid);

        public static IDeviceFactory GetHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory)
            => filterDeviceDefinitions.CreateWindowsHidDeviceFactory(loggerFactory);

        public static IDeviceFactory GetHidDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory,
            Func<Report, TransferResult> readReportTransform = null,
            WriteReportTransform writeReportTransform = null
            )
            => filterDeviceDefinition.CreateWindowsHidDeviceFactory(
                loggerFactory,
                readReportTransform: readReportTransform,
                writeReportTransform: writeReportTransform,
                createReadConnection: (apiService, deviceId, fileAccessRights, shareMode, creationDisposition)
                => apiService.CreateFile(
                    deviceId,
                    Windows.FileAccessRights.GenericRead,
                    shareMode,
                    IntPtr.Zero,
                    creationDisposition,
                    FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero));
    }
}

