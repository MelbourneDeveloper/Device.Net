For Hid Devices:

**Install-Package Hid.Net**

For Usb Devices on Android, Windows, or UWP:

**Install-Package Usb.Net**

For Usb Devices on Linux or MacOS

**Install-Package Device.Net.LibUsb**

Device.Net only provides the base interface. This would allow you to create a provider for a new device type like Bluetooth for example.