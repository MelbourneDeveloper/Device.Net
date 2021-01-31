
# Hid.Net, Usb.Net, SerialPort.Net (Device.Net)

![diagram](https://github.com/MelbourneDeveloper/Device.Net/blob/main/Diagram.png)

**Cross-platform .NET framework for talking to connected devices such as USB, Serial Port and Hid devices**

Version 4 is live on [Nuget.org](https://www.nuget.org/packages/Device.Net)! Take a look at the [4.0 project](https://github.com/MelbourneDeveloper/Device.Net/projects/11) to see new features and fixes. Version 4 has public interface changes. You will need to read through the documentation to upgrade from version 3 to version 4.

This framework provides a common Task async programming interface across platforms and device types. This allows for dependency injection to use different types of devices on any platform with the same code. The supported device types are Hid, Serial Port, and USB. 

### Contribute
This project needs funding. Please [sponsor me here](https://github.com/sponsors/MelbourneDeveloper) so that I can contribute more time to improving this framework.

| Coin           | Address |
| -------------  |:-------------:|
| Bitcoin        | [33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U](https://www.blockchain.com/btc/address/33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U) |
| Ethereum       | [0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e](https://etherdonation.com/d?to=0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e) |
| Litecoin       | MVAbLaNPq7meGXvZMU4TwypUsDEuU6stpY |

This project also needs unit tests, bug fixes and work towards more platforms. Please [read this](https://github.com/MelbourneDeveloper/Device.Net/blob/main/CONTRIBUTING.md).

#### Licensing

This framework uses the [MIT license](https://github.com/MelbourneDeveloper/Device.Net/blob/main/LICENSE). I won't sue you, or your business if you use this for free. If you are developing software for free, I don't expect you to sponsor me. However, if your business makes more than USD 100,000 per year and your software depends on Device.Net, I expect your business to make a serious contribution via [sponsorship](https://github.com/sponsors/MelbourneDeveloper). 

### Why Device.Net?

Device communication is fragmented across platforms and device types. If you need to use three different device types across Android, UWP and .NET, you would otherwise need nine different APIs. Device.Net puts a standard layer across all these so that you can share code across all platforms and device types. You don't need to use Windows APIs or learn about Android's API directly. If the device manufacturer decides to switch from USB to Hid, the code remains the same. Write once; run everywhere.

### Get Help

* [Follow me on Twitter](https://twitter.com/intent/follow?screen_name=cfdevelop&tw_p=followbutton)

* [Documentation](https://melbournedeveloper.github.io/Device.Net/index.html)

* [Quick Start](https://melbournedeveloper.github.io/Device.Net/articles/GettingStarted.html)

* [Join the conversation](https://discord.gg/ZcvXARm) on Discord

* Check out [my blog](https://christianfindlay.com/) for articles

* [Nuget packages](https://melbournedeveloper.github.io/Device.Net/articles/NuGet.html)

### Currently supports:

| Platform       | Hid | USB                                                                                                 | Serial Port | Bluetooth |
|----------------|:---:|-----------------------------------------------------------------------------------------------------|-------------|-----------|
| .NET Framework | Yes | Yes                                                                                                 | Yes         | No        |
| .NET Core      | Yes | Yes                                                                                                 | Yes         | No        |
| Android        | Yes | Yes                                                                                                 | No          | No        |
| UWP            | Yes | Yes                                                                                                 | No          | No        |
| Linux, MacOS*  |  No | [(Via LibUsbDotNet)](https://github.com/MelbourneDeveloper/Device.Net/wiki/Linux-and-MacOS-Support) | No          | No        |
| WebAssembly    | No  | No                                                                                                  | No          | No        |

*Note: Bluetooth, Linux, and macOS, WebAssembly (via [WebUsb](https://web.dev/usb/)) support are on the radar. If you can sponsor this project, you might be able to help get there faster.*

*SerialPort.Net and Device.Net.LibUsb are still in alpha mode. You must use the prerelease version*

### Example Code

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
            //Create logger factory that will pick up all logs and output them in the debug output window
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
            var deviceDefinitions = (await factories.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false)).ToList();

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

### See Also

[Human Interface Device Wikipedia Page](https://en.wikipedia.org/wiki/Human_interface_device) - Good for understanding the difference between the meaning of the two terms: USB and Hid.

[USB human interface device class Wikipedia Page](https://en.wikipedia.org/wiki/USB_human_interface_device_class) - as above

[USB Wikipedia Page](https://en.wikipedia.org/wiki/USB) - as above

Jax Axelson's [USB Page](http://janaxelson.com/usb.htm) - General C# USB Programming

