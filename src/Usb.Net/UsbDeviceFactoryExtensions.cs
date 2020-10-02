using Device.Net;
using Microsoft.Extensions.Logging;
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
                var usbInterfaceManager = await getUsbInterfaceManager(d);
                return new UsbDevice(d, usbInterfaceManager, loggerFactory);
            },
            DeviceType.Usb);
    }
}

