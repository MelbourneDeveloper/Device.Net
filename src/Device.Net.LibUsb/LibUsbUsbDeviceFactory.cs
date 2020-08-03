using Microsoft.Extensions.Logging;
using System;

namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public LibUsbUsbDeviceFactory(ILoggerFactory loggerFactory, ITracer tracer) : base(loggerFactory, tracer)
        {
        }

        public override DeviceType DeviceType => DeviceType.Usb;

        /// <summary>
        /// Register the factory for enumerating USB devices.
        /// </summary>
        [Obsolete(DeviceManager.ObsoleteMessage)]
        public static void Register(ILoggerFactory loggerFactory, ITracer tracer) => DeviceManager.Current.DeviceFactories.Add(new LibUsbUsbDeviceFactory(loggerFactory, tracer));
    }
}
