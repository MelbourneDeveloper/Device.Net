# Device.Net

![Device.Net Logo](images/Diagram.png)

**Cross-platform .NET framework for talking to connected devices such as USB, Serial Port, and Hid devices uniformly through dependency injection.**

This framework provides a common task-based Async interface across platforms and device types. This allows for dependency injection to use different types of devices on any platform with the same code. The supported device types are Hid, Serial Port, and USB. We will add Bluetooth in the future, and we are looking for Bluetooth programmers to help. Please join the community on Discord [here](https://discord.gg/ZcvXARm).

## Hid.Net
This is a cross-platform library for talking to HID devices. The library targets .NET and UWP.

## Usb.Net
This is a cross-platform library for talking to USB devices. The library targets .NET, Android, and UWP.

## SerialPort.Net
This is a .NET library for talking to Serial Port / COM devices on Windows. We are looking to expand this library to other platforms soon.

## Device.Net.LibUsb
This is a macOS, Windows, and Linux library for talking to USB devices. This uses the library [LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet) to bridge with Device.Net. This is an underdeveloped library, and we are looking to replace this with native Linux and macOS API calls. Please reach out if you can help.

### Currently supports:

| Platform | Device Types |
| ------------- |:-------------:|
| .NET Framework     | Hid, USB, Serial Port |
| .NET Core / .NET 5     | Hid, USB, Serial Port  |
| Android* | Hid, USB |
| UWP | Hid, USB   |
| Linux, MacOS* | [USB (Via LibUsbDotNet)](https://github.com/MelbourneDeveloper/Device.Net/wiki/Linux-and-MacOS-Support)  |

_Note: Android treats Hid devices and USB devices as the same thing_