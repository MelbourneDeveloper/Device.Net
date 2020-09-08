using Device.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Usb.Net
{
    public delegate Task<IUsbInterfaceManager> GetUsbInterfaceManager(string deviceId);

    public static class UsbDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateUsbDeviceFactory(
        ILoggerFactory loggerFactory,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync,
        GetUsbInterfaceManager getUsbInterfaceManager)
        => loggerFactory == null
            ? throw new ArgumentNullException(nameof(loggerFactory))
            :
            new DeviceFactory(
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            async (d) =>
            {
                var usbInterfaceManager = await getUsbInterfaceManager(d);
                return new UsbDevice(d, usbInterfaceManager, loggerFactory);
            });
    }
}

