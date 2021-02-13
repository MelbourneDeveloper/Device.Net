using Device.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    public delegate Task<IUsbInterfaceManager> GetUsbInterfaceManager(string deviceId, CancellationToken cancellationToken = default);

    public static class UsbDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateUsbDeviceFactory(
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync,
        GetUsbInterfaceManager getUsbInterfaceManager,
        ILoggerFactory? loggerFactory = null,
        Guid? classGuid = null)
        =>
            new DeviceFactory(
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            async (d, cancellationToken) =>
            {
                var usbInterfaceManager = await getUsbInterfaceManager(d.DeviceId, cancellationToken).ConfigureAwait(false);
                return new UsbDevice(d.DeviceId, usbInterfaceManager, loggerFactory);
            },
            //Support the device if the factory doesn't filter on class guid, or the filter matches the device
            (c, cancellationToken) => Task.FromResult(c.DeviceType == DeviceType.Usb && (classGuid == null || classGuid.Value == c.ClassGuid))
            );
    }
}

