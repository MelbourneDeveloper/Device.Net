using Android.Content;
using Android.Hardware.Usb;
using AndroidUsbDevice = Android.Hardware.Usb.UsbDevice;
using Device.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Android
{
    public static class AndroidUsbFactoryExtensions
    {
        public static IDeviceFactory CreateAndroidUsbDeviceFactory(
        this FilterDeviceDefinition filterDeviceDefinition,
        UsbManager usbManager,
        Context context,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetUsbInterfaceManager getUsbInterfaceManager = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        IAndroidFactory androidFactory = null,
        Func<AndroidUsbDevice, IUsbPermissionBroadcastReceiver> getUsbPermissionBroadcastReceiver = null
        )
        {
            return CreateAndroidUsbDeviceFactory(
                new ReadOnlyCollection<FilterDeviceDefinition>(new List<FilterDeviceDefinition> { filterDeviceDefinition }),
                usbManager,
                context,
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                getUsbInterfaceManager,
                readBufferSize,
                writeBufferSize,
                androidFactory,
                getUsbPermissionBroadcastReceiver);
        }

        public static IDeviceFactory CreateAndroidUsbDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        UsbManager usbManager,
        Context context,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetUsbInterfaceManager getUsbInterfaceManager = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        IAndroidFactory androidFactory = null,
        Func<AndroidUsbDevice, IUsbPermissionBroadcastReceiver> getUsbPermissionBroadcastReceiver = null
        )
        {
            if (usbManager == null) throw new ArgumentNullException(nameof(usbManager));
            if (context == null) throw new ArgumentNullException(nameof(context));
            loggerFactory ??= NullLoggerFactory.Instance;

#if __ANDROID__
            if (androidFactory == null)
            {
                androidFactory = new AndroidFactory();
            }

            if (getUsbPermissionBroadcastReceiver == null)
            {
                getUsbPermissionBroadcastReceiver = new Func<AndroidUsbDevice, IUsbPermissionBroadcastReceiver>((ud) =>
                    new UsbPermissionBroadcastReceiver(
                    usbManager,
                    ud,
                    context,
                    androidFactory,
                    loggerFactory.CreateLogger<UsbPermissionBroadcastReceiver>()));
            }
#else
            if (androidFactory == null)
            {
                throw new ArgumentNullException(nameof(androidFactory));
            }

            if (getUsbPermissionBroadcastReceiver == null)
            {
                throw new ArgumentNullException(nameof(getUsbPermissionBroadcastReceiver));
            }
#endif

            getConnectedDeviceDefinitionsAsync ??= (cancellationToken) =>
                {
                    return Task.FromResult<IEnumerable<ConnectedDeviceDefinition>>
                    (
                        new ReadOnlyCollection<ConnectedDeviceDefinition>
                        (
                            usbManager
                            .DeviceList
                            .Select(kvp => kvp.Value)
                            .Where
                            (
                                d => filterDeviceDefinitions
                                .FirstOrDefault
                                (
                                    f =>
                                        (!f.VendorId.HasValue || f.VendorId.Value == d.VendorId) &&
                                        (!f.ProductId.HasValue || f.ProductId.Value == d.ProductId)
                                ) != null
                            )
                            .Select(AndroidUsbInterfaceManager.GetAndroidDeviceDefinition)
                            .ToList()
                        )
                    );
                };

            getUsbInterfaceManager ??= (a, cancellationToken) => Task.FromResult<IUsbInterfaceManager>(
                new AndroidUsbInterfaceManager(
                    usbManager,
                    context,
                    //TODO: throw a validation message
                    int.Parse(a, IntParsingCulture),
                    androidFactory,
                    getUsbPermissionBroadcastReceiver,
                    loggerFactory,
                    readBufferSize,
                    writeBufferSize
                ));

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager, loggerFactory);
        }

        internal static CultureInfo IntParsingCulture { get; } = CultureInfo.GetCultureInfo("en-US");
    }
}