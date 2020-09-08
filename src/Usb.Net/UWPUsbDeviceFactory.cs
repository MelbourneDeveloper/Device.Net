using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Usb.Net.UWP
{
    public delegate Task<IUsbInterfaceManager> GetUsbInterfaceManager(string deviceId);

    public static class UsbDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateUsbDeviceFactory(
            ILoggerFactory loggerFactory,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync,
            GetUsbInterfaceManager getUsbInterfaceManager)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger<UwpDeviceEnumerator>();

            return new DeviceFactory(
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                async (d) =>
                {
                    var usbInterfaceManager = await getUsbInterfaceManager(d.DeviceId);
                    return new UsbDevice(d.DeviceId, usbInterfaceManager, loggerFactory);
                });
        }
    }
}

