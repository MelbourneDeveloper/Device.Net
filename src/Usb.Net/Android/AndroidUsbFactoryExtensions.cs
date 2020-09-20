using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net.Android
{
    public static class AndroidUsbFactoryExtensions
    {
        public static IDeviceFactory CreateAndroidUsbDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory,
            UsbManager usbManager,
            Context context,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetUsbInterfaceManager getUsbInterfaceManager = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
            )
        {
            if (getConnectedDeviceDefinitionsAsync == null)
            {
                getConnectedDeviceDefinitionsAsync = async () =>

                     usbManager.DeviceList.Select(kvp => kvp.Value).Where(d
                     =>
                         filterDeviceDefinitions.FirstOrDefault(f
                            =>
                            (!f.VendorId.HasValue || f.VendorId.Value == d.VendorId) &&
                            (!f.ProductId.HasValue || f.ProductId.Value == d.ProductId)
                        ) != null

                    ).Select(AndroidUsbInterfaceManager.GetAndroidDeviceDefinition);
            }

            if (getUsbInterfaceManager == null)
            {
                getUsbInterfaceManager = async (a) => new AndroidUsbInterfaceManager(
                    usbManager,
                    context,
                    //TODO: throw a validation message
                    int.Parse(a),
                    loggerFactory,
                    readBufferSize,
                    writeBufferSize
                    );
            }

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(loggerFactory, getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager);
        }
    }
}