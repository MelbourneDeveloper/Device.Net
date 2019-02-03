# Hid.Net, Usb.Net (Device.Net)

![diagram](https://github.com/MelbourneDeveloper/Device.Net/blob/master/Diagram.png)

**Cross platform C# library for talking to connected devices such as Usb, and Hid devices.**

**Linux and MacOS support is here! Check out the [MacOS](https://github.com/MelbourneDeveloper/Device.Net/tree/develop/src/Device.Net.MacOSLibUsbSample) sample in the develop branch**

This library provides a common Task based Async interface across platforms and device types. This allows for dependency injection so that different types of devices can be used on any platform with the same code. The supported device types are Hid, and USB. Other device types such as Bluetooth and so on may be added in future. Hid.Net is specifically for Hid devices that may be Usb devices. Usb.Net is specifically for Usb devices that don't have a Hid interface.

Please visit the [documentation page](https://github.com/MelbourneDeveloper/Device.Net/wiki).

### Currently supports:

| Platform | Device Types |
| ------------- |:-------------:|
| .NET Framework     | Hid, USB |
| .NET Core      | Hid, USB  |
| Android | USB |
| UWP | Hid, USB   |
| Linux* | USB (Via LibUsbDotNet)  |

*See [Linux and MacOS Support](https://github.com/MelbourneDeveloper/Device.Net/wiki/Linux-and-MacOS-Support)

## [Quick Start](https://github.com/MelbourneDeveloper/Device.Net/wiki/Quick-Start)

Example Code:
```cs
    private static async Task InitializeTrezor()
    {
        //Register the factory for creating Usb devices. This only needs to be done once.
        UWPUsbDeviceFactory.Register();

        //Register the factory for creating Usb devices. This only needs to be done once.
        UWPHidDeviceFactory.Register();

        //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
        var deviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x" },
            new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C1, Label="Trezor One Firmware 1.7.x" },
            new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C0, Label="Model T" }
        };

        //Get the first available device and connect to it
        var devices = await DeviceManager.Current.GetDevices(deviceDefinitions);
        var trezorDevice = devices.FirstOrDefault();
        await trezorDevice.InitializeAsync();

        //Create a buffer with 3 bytes (initialize)
        var buffer = new byte[64];
        buffer[0] = 0x3f;
        buffer[1] = 0x23;
        buffer[2] = 0x23;

        //Write the data to the device
        await trezorDevice.WriteAsync(buffer);

        //Read the response
        var readBuffer = await trezorDevice.ReadAsync();
    }
```
## Donate

All these libraries are open source and free. I am not endorsed, or funded by any of the manufacturers of the devices I develop for. Your donations will contribute to making sure that these libraries keep up with the latest firmware, functions are implemented, and the quality is maintained.

Bitcoin: 33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U

Ethereum: 0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e

Litecoin: MVAbLaNPq7meGXvZMU4TwypUsDEuU6stpY

## [Samples & Unit Tests](https://github.com/MelbourneDeveloper/Device.Net/wiki/Samples-and-Unit-Tests)

## Store App Production Usage

**Hardfolio** - Cryptocurrency portfolio app for hardwarewallets. Hid.Net started its life as a project inside the Hardfolio app codebase. The original aim of this app was to support multiple hardwarewallets across multiple platforms. It turned out that Hid.Net and Usb.Net were warranted as libraries in their own right because there really is not other library on the internet that supports all the platforms that were needed for Hardfolio.

[Google Play](https://play.google.com/store/apps/details?id=com.Hardfolio)

[Windows Store](https://www.microsoft.com/en-au/p/hardfolio/9p8xx70n5d2j)

## [NuGet](https://github.com/MelbourneDeveloper/Device.Net/wiki/NuGet)

## Contact

Join us on [Slack](https://hardwarewallets.slack.com)

Follow me on [Twitter](https://twitter.com/HardfolioApp)

Read my [blog](https://christianfindlay.wordpress.com)

## [Contribution](https://github.com/MelbourneDeveloper/Device.Net/blob/master/CONTRIBUTING.md)

I welcome feedback, and pull requests. If there's something that you need to change in the library, please log an issue, and explain the problem. If you have a proposed solution, please write it up and explain why you think it is the answer to the problem. The best way to highlight a bug is to submit a pull request with a unit test that fails so I can clearly see what the problem is in the first place.

### Pull Requests

Please break pull requests up in to their smallest possible parts. If you have a small feature of refactor that other code depends on, try submitting that first. Please try to reference an issue so that I understand the context of the pull request. If there is no issue, I don't know what the code is about. If you need help, please jump on Slack here: https://hardwarewallets.slack.com

### See Also

[Human Interface Device Wikipedia Page](https://en.wikipedia.org/wiki/Human_interface_device) - Good for understanding the difference between the meaning of the two terms: USB and Hid.

[USB human interface device class Wikipedia Page](https://en.wikipedia.org/wiki/USB_human_interface_device_class) - as above

[USB Wikipedia Page](https://en.wikipedia.org/wiki/USB) - as above

Jax Axelson's [USB Page](http://janaxelson.com/usb.htm) - General C# USB Programming
