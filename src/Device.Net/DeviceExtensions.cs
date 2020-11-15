using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net
{
    public static class DeviceExtensions
    {
        public static IDeviceFactory Aggregate(this IList<IDeviceFactory> deviceFactories, ILoggerFactory loggerFactory = null)
            => deviceFactories == null ? throw new ArgumentNullException(nameof(deviceFactories)) :
            new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(deviceFactories), loggerFactory);

        public static DeviceDataStreamer CreateDeviceDataStreamer(
    this IDeviceFactory deviceFactory,
    ProcessData processData,
    Func<IDevice, Task> initializeFunc = null) =>
    new DeviceDataStreamer(
        processData,
        deviceFactory,
        initializeFunc: initializeFunc);

        public static bool IsDefinitionMatch(this FilterDeviceDefinition filterDevice, ConnectedDeviceDefinition actualDevice, DeviceType deviceType)
        {
            if (actualDevice == null) throw new ArgumentNullException(nameof(actualDevice));

            if (filterDevice == null) return true;

            var vendorIdPasses = !filterDevice.VendorId.HasValue || filterDevice.VendorId == actualDevice.VendorId;
            var productIdPasses = !filterDevice.ProductId.HasValue || filterDevice.ProductId == actualDevice.ProductId;
            var deviceTypePasses = actualDevice.DeviceType == deviceType;
            var usagePagePasses = !filterDevice.UsagePage.HasValue || filterDevice.UsagePage == actualDevice.UsagePage;

            var returnValue =
                vendorIdPasses &&
                productIdPasses &&
                deviceTypePasses &&
                usagePagePasses;

            return returnValue;
        }
    }
}