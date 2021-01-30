## The Basics
To enumerate connected devices, you must register the factories for the device types. A factory is a class that implements `IDeviceFactory`. If you're unsure which device type you want to connect to, you should try both USB and Hid. In this case, you will need to add both [NuGet](https://melbournedeveloper.github.io/Device.Net/articles/NuGet.html) packages. See the getting started guide for creating factories. Creating factories is always platform-specific. One approach is to create these in the composition root of your app. Your cross-platform shared code will not include factory creation. `IDeviceFactory` is a simple interface that puts a layer over the top of listing devices and instantiating them.  Instantiate the factories by calling `Create-` on a FilterDeviceDefinition or an IEnumerable<> of them. E.g.

```cs
new FilterDeviceDefinition(vendorId: 0x534C, productId: 0x0001, label: "Trezor One Firmware 1.6.x", usagePage: 65280)
                .CreateWindowsHidDeviceFactory(loggerFactory);
```

Call and await `GetConnectedDeviceDefinitionsAsync()` on the factory to enumerate the devices connected to the computer. The get one of the devices, you need to call `GetDeviceAsync()` on the factory and pass in a definition. This will return an `IDevice`, and you need to call `InitializeAsync()` to use the device. 

_Note: it is a good idea to specify a logger during the factory registration. This means that factories will log errors and so on when attempting to connect to enumerate or connect to devices. See the section Debugging, Logging and Tracing.

_If you have not already been through the process you will need to [configure device permissions](https://melbournedeveloper.github.io/Device.Net/articles/DevicePermissionSetup.html) on Android, or UWP._

## Advanced Workflows

Device.Net supports multiple workflows. You may expect your user to connect the device before the app starts and leave it connected, or the user may plug and unplug the device several times. Any approach is possible, but different tools facilitate different approaches. 

### Single Device UI

This is probably the most common UI workflow that you will implement. An app starts up and shows a wait indicator until a device is connected. The device may be a gamepad, hardwarewallet, or anything that exchanges data with the computer. The app may accept multiple models or Product Ids, but the app works in the same way. Perhaps some functionality is toggled on or off based on the Product Id. The single device interacts with the controls on the screen, and then the UI becomes locked once the device is disconnected. In this scenario, you may have a Combobox to switch between multiple devices if the user connects multiple. 

There are two classes for this: `DeviceManager`, and `DeviceListener`. The former is the recommended class, but it is a work in progress and not included in the Device.Net Nuget package. `DeviceManager` aims at asynchronous messaging while `DeviceListener` uses traditional .NET events. See the documentation for these classes.

### Data Streaming

Some devices don't require you to send complicated messages to the device. In these cases, the device streams data to your computer, but you only send minimal messaging to the device. A good example is a thermometer. Generally, you only send a request for the current temperature, and it returns the result. It does this again and again. In this case, you connect to the device, send a message in a loop, and do something with the result.  

This example uses [.NET reactive extensions](https://github.com/dotnet/reactive) to achieve this. The first step gets the first device and then connects to it. The, we create an observable which is configured to poll the device every 100 milliseconds.

```cs
private static async Task DisplayTemperature()
{
    //Connect to the device by product id and vendor id
    var temperDevice = await new FilterDeviceDefinition(vendorId: 0x413d, productId: 0x2107, usagePage: 65280)
        .CreateWindowsHidDeviceFactory(_loggerFactory)
        .ConnectFirstAsync()
        .ConfigureAwait(false);

    //Create the observable
    var observable = Observable
        .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(.1))
        .SelectMany(_ => Observable.FromAsync(() => temperDevice.WriteAndReadAsync(new byte[] { 0x00, 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 })))
        .Select(data => (data.Data[4] & 0xFF) + (data.Data[3] << 8))
        //Only display the temperature when it changes
        .Distinct()
        .Select(temperatureTimesOneHundred => Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven));

    //Subscribe to the observable
    _ = observable.Subscribe(t => Console.WriteLine($"Temperature is {t}"));

    //Note: in a real scenario, we would dispose of the subscription afterwards. This method runs forever.
}
```