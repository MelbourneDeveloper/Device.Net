using Device.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Hid.Net.UWP
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public static class UWPDeviceFactoryExtensions
    {

        public static IDeviceFactory CreateUwpHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory,
            GetDevice getDevice)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var asdasd = new UwpHidDeviceEnumerator(loggerFactory, loggerFactory.CreateLogger<UwpHidDeviceEnumerator>());

            return new DeviceFactory(
                loggerFactory,
                asdasd.GetConnectedDeviceDefinitionsAsync,
                getDevice);
        }
    } 
}
