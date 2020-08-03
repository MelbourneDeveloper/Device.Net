using System;

namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public LibUsbUsbDeviceFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }

        public override DeviceType DeviceType => DeviceType.Usb;

        /// <summary>
        /// Register the factory for enumerating USB devices.
        /// </summary>
        [Obsolete(DeviceManager.ObsoleteMessage)]
        public static void Register(ILogger logger, ITracer tracer) => DeviceManager.Current.DeviceFactories.Add(new LibUsbUsbDeviceFactory(logger, tracer));
    }
}
