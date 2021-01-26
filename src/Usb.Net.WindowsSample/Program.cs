using Device.Net;
using Hid.Net.Windows;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Usb.Net.WindowsSample
{
    internal class Program
    {
        private static async Task Main()
        {
            var loggerFactory = LoggerFactory.Create((builder) =>
            {
                _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
            });

            //----------------------

            // This is Windows specific code. You can replace this with your platform of choice or put this part in the composition root of your app

            //Register the factory for creating Hid devices. 
            var hidFactory =
                new FilterDeviceDefinition(vendorId: 0x534C, productId: 0x0001, label: "Trezor One Firmware 1.6.x", usagePage: 65280)
                .CreateWindowsHidDeviceFactory(loggerFactory);

            //Register the factory for creating Usb devices.
            var usbFactory =
                new FilterDeviceDefinition(vendorId: 0x1209, productId: 0x53C1, label: "Trezor One Firmware 1.7.x")
                .CreateWindowsUsbDeviceFactory(loggerFactory);

            //----------------------

            //Join the factories together so that it picks up either the Hid or USB device
            var factories = hidFactory.Aggregate(usbFactory);

            //Get connected device definitions
            var deviceDefinitions = (await hidFactory.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false)).ToList();

            if (deviceDefinitions.Count == 0)
            {
                //No devices were found
                return;
            }

            //Get the device from its definition
            var trezorDevice = await hidFactory.GetDeviceAsync(deviceDefinitions.First()).ConfigureAwait(false);

            //Initialize the device
            await trezorDevice.InitializeAsync().ConfigureAwait(false);

            //Create the request buffer
            var buffer = new byte[65];
            buffer[0] = 0x00;
            buffer[1] = 0x3f;
            buffer[2] = 0x23;
            buffer[3] = 0x23;

            //Write and read the data to the device
            var readBuffer = await trezorDevice.WriteAndReadAsync(buffer).ConfigureAwait(false);
        }
    }
}
