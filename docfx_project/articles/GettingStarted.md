# Getting Started

## Run Windows Sample (Usb.Net.WindowsSample)

Run the Windows sample. You should see your device's path appear at the top of the console. If it does not, check the logs to find out why it didn't appear.

![Image](../images/WindowsSample.png)

## Modify a Sample

The easiest way to start is to clone this repo that uses Usb.Net or Hid.Net and modify the code to suit your needs. This repo contains a sample for Hid thermometer device and a Trezor Hardware wallet example.  If you have trouble getting a sample to compile, please see [Build Issues](BuildIssues.md)

## Clean Project

If you want to start fresh, the easiest way would be to create a .NET Core project on Windows with Visual Studio and add the NuGet packages you require. Please see the [NuGet page](NuGet.md).

Please see 

[Device Permission Setup](DevicePermissionSetup.md)

[Device Listener](DeviceListener.md) or [Enumerating Connected Devices](EnumeratingConnectedDevices.md) 

[Writing To and Reading From a Device](WritingToandReadingFromaDevice.md)

For USB, please see [USB-Initialization: Interfaces And Endpoints](USBInitialization.md)

### Example Code:

This is UWP code. The only difference for Windows is that you would call WindowsHidDeviceFactory.Register().

```cs
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

```