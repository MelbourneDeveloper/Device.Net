using Device.Net;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Usb.Net
{
    public delegate Task<IUsbInterfaceManager> GetUsbInterfaceManager(string deviceId);

    public static class UsbDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateUsbDeviceFactory(
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync,
        GetUsbInterfaceManager getUsbInterfaceManager,
        ILoggerFactory loggerFactory = null)
        =>
            new DeviceFactory(
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            async d =>
            {
                var usbInterfaceManager = await getUsbInterfaceManager(d.DeviceId);
                return new UsbDevice(d.DeviceId, usbInterfaceManager, loggerFactory);
            },
            new ReadOnlyCollection<DeviceType>(new List<DeviceType> { DeviceType.Usb })
            );
    }
}

