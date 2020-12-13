To enumerate connected devices, you must register the factories for the device types. If you're not sure which device type you want to connect to, you should try both USB, and Hid. In this case, you will need to add both [NuGet](https://github.com/MelbourneDeveloper/Device.Net/wiki/NuGet) packages. Using the [DeviceListener](https://github.com/MelbourneDeveloper/Device.Net/wiki/Device-Listener) is probably a better option than simply enumerating devices because it will handle the connecting to (initializing) and from the device for you.

_Note: if you have not already been through the process you will need to [configure device permissions](https://github.com/MelbourneDeveloper/Device.Net/wiki/Device-Permission-Setup) on Android, or UWP._

Here is an example for registering both factories on Windows:

```cs
WindowsUsbDeviceFactory.Register(Logger, Tracer);
WindowsHidDeviceFactory.Register(Logger, Tracer);
```

_Note: it is a good idea to specify a logger during the factory registration. This means that the factories will log errors and so on when attempting to connect to enumerate or connect to devices. The [DebugLogger](https://github.com/MelbourneDeveloper/Device.Net/blob/master/src/Device.Net/DebugLogger.cs) is a simple example. This will log information to the debug window. More sophisticated logging can be implemented by implementing ILogger._

[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/3a7324746e01a0e252d2a4d1b630ed4b623f3903/src/Usb.Net.WindowsSample/Program.cs#L105)

Then, you can use the Device manager to get a list of connected devices.

```cs
var devices = await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null);
Console.WriteLine("Currently connected devices: ");
foreach (var device in devices)
{
    Console.WriteLine(device.DeviceId);
}
Console.WriteLine();
```

**Output:**
> \\?\usb#vid_1209&pid_53c1&mi_00#6&3344a6c7&1&0000#{dee824ef-729b-4a0e-9c14-b7117d33a817}

> \\?\hid#vid_1209&pid_53c1&mi_01#7&317d5c08&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}

As you can see above, these are unique Ids that will allow you to construct IDevice objects that can communicate with the Hid or USB device. In this case, the device with a Vid of 0x1209 and Pid of 0x53C1 has both a USB and a Hid interface. You can construct a device and initialize simply like this:

```cs
var windowsUsbDevice = new WindowsUsbDevice(devices.First().DeviceId);
await windowsUsbDevice.InitializeAsync();
```

**Error Logging**

See [this article](https://github.com/MelbourneDeveloper/Device.Net/wiki/Debugging,-Logging,-and-Tracing)

If devices fail for some reason, the library will try to log the result. However, for logging to be enabled, an ILogger object must be passed with the factory registration like so:

[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/3a7324746e01a0e252d2a4d1b630ed4b623f3903/src/Usb.Net.WindowsSample/Program.cs#L30)
```cs
WindowsUsbDeviceFactory.Register(Logger, Tracer);
```
The Windows sample uses the default DebugLogger which will simply log results to the debug output window. This will provide useful information about why a device may not connect even though the Device Id appears in the list of Device Ids.

**Using DeviceManager**

Alternatively, you can allow the DeviceManager to create your devices like this. This is generally the easier option.

```cs
//Define the types of devices to search for.
var deviceDefinitions = new List<FilterDeviceDefinition>
{
    new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C1, Label="Trezor One Firmware 1.7.x" }
};

//Get the first available device and connect to it
var devices = await DeviceManager.Current.GetDevicesAsync(_DeviceDefinitions);
using (var trezorDevice = devices.FirstOrDefault())
{
    await trezorDevice.InitializeAsync();
}
```

[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/a63ed3781b16f18dbefed13c6f0c9215377cceaa/src/Usb.Net.UWP.Sample/TrezorExample.cs#L70)