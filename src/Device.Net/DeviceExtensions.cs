using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net
{
    public static class DeviceExtensions
    {
        /// <summary>
        /// Creates a new <see cref="IDeviceFactory"/> that contains with the existing deviceFactories
        /// </summary>
        /// <param name="deviceFactories"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public static IDeviceFactory Aggregate(this IList<IDeviceFactory> deviceFactories, ILoggerFactory? loggerFactory = null)
            => deviceFactories == null ? throw new ArgumentNullException(nameof(deviceFactories)) :
            new AggregateDeviceFactory(new ReadOnlyCollection<IDeviceFactory>(deviceFactories), loggerFactory);


        public static IDeviceFactory Aggregate(this IDeviceFactory deviceFactory, IDeviceFactory newDeviceFactory, ILoggerFactory? loggerFactory = null)
            => deviceFactory == null ? throw new ArgumentNullException(nameof(deviceFactory)) :
            new AggregateDeviceFactory(
                new ReadOnlyCollection<IDeviceFactory>(
                    new ReadOnlyCollection<IDeviceFactory>(
                        new List<IDeviceFactory> { deviceFactory, newDeviceFactory })), loggerFactory);

        /// <summary>
        /// Compares a <see cref="ConnectedDeviceDefinition"/> with a <see cref="FilterDeviceDefinition"/>
        /// </summary>
        /// <param name="filterDevice"></param>
        /// <param name="actualDevice"></param>
        /// <param name="deviceType"></param>
        /// <returns>True if the filterDevice matches the actualDevice</returns>
        public static bool IsDefinitionMatch(this FilterDeviceDefinition filterDevice, ConnectedDeviceDefinition actualDevice, DeviceType deviceType)
        {
            if (actualDevice == null) throw new ArgumentNullException(nameof(actualDevice));

            if (filterDevice == null) return true;

            var vendorIdPasses = !filterDevice.VendorId.HasValue || filterDevice.VendorId == actualDevice.VendorId;
            var productIdPasses = !filterDevice.ProductId.HasValue || filterDevice.ProductId == actualDevice.ProductId;
            var deviceTypePasses = actualDevice.DeviceType == deviceType;
            var usagePagePasses = !filterDevice.UsagePage.HasValue || filterDevice.UsagePage == actualDevice.UsagePage;
            var classGuidPasses = !filterDevice.ClassGuid.HasValue || filterDevice.ClassGuid == actualDevice.ClassGuid;

            var returnValue =
                vendorIdPasses &&
                productIdPasses &&
                deviceTypePasses &&
                usagePagePasses &&
                classGuidPasses;

            return returnValue;
        }

        public static async Task<IDevice> GetFirstDeviceAsync(this IDeviceFactory deviceFactory)
            => deviceFactory != null ?
            await deviceFactory.GetDeviceAsync(await (await deviceFactory.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false)).ConfigureAwait(false)
            : throw new ArgumentNullException(nameof(deviceFactory));

        public static async Task<IDevice> ConnectFirstAsync(this IDeviceFactory deviceFactory, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            var device = await GetFirstDeviceAsync(deviceFactory).ConfigureAwait(false);

            if (device == null)
            {
                var deviceException = new DeviceException(Messages.ErrorMessageCouldntGetDevice);

                logger.LogError(deviceException, "No devices found");

                throw deviceException;
            }

            await device.InitializeAsync().ConfigureAwait(false);
            return device;
        }

    }
}