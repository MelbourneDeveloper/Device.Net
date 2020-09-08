using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Device.Net.UWP
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public static class UWPHidDeviceFactoryExtensions
    {

        public static IDeviceFactory CreateUwpHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory,
            GetDevice getDevice)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var uwpHidDeviceEnumerator = new UwpHidDeviceEnumerator(loggerFactory, loggerFactory.CreateLogger<UwpDeviceEnumerator>());

            return new DeviceFactory(
                loggerFactory,
                uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync,
                getDevice);
        }
    } 
}
