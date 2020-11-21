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
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync,
        GetUsbInterfaceManager getUsbInterfaceManager,
        ILoggerFactory loggerFactory = null,
        Guid? classGuid = null)
        =>
            new DeviceFactory(
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            async d =>
            {
                var usbInterfaceManager = await getUsbInterfaceManager(d.DeviceId);
                return new UsbDevice(d.DeviceId, usbInterfaceManager, loggerFactory);
            },
            //Support the device if the factory doesn't filter on class guid, or the filter matches the device
            async (c) => c.DeviceType == DeviceType.Usb && (classGuid == null || classGuid.Value == c.ClassGuid)
            );
    }
}

