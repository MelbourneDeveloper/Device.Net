A device listener is a platform independent class that will handle connecting and disconnecting from devices that are connected to the computer. Firstly, you must specify the filter definitions for the devices you want to connect to, and then the device listener will handle the rest. This works exactly the same way across each platform, but in order to make this work, the platform specific device factories must be registered.

Here is an example for registering both factories on Windows:

```cs
WindowsUsbDeviceFactory.Register(Logger, Tracer);
WindowsHidDeviceFactory.Register(Logger, Tracer);
```

_Note: if you have not already been through the process you will need to [configure device permissions](https://github.com/MelbourneDeveloper/Device.Net/wiki/Device-Permission-Setup) on Android, or UWP._

Here is a working example that listens for a device and notifies you when the device is connected or disconnected. All three sample applications (Android, UWP, Windows) use this same code.

```cs
internal sealed class TrezorExample : IDisposable
{
    #region Fields
#if(LIBUSB)
    private const int PollMilliseconds = 6000;
#else
    private const int PollMilliseconds = 3000;
#endif
    //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
    private readonly List<FilterDeviceDefinition> _DeviceDefinitions = new List<FilterDeviceDefinition>
    {
        new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x", UsagePage=65280 },
        new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x (Android Only)" },
        new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C1, Label="Trezor One Firmware 1.7.x" },
        new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C0, Label="Model T" }
    };
    #endregion

    #region Events
    public event EventHandler TrezorInitialized;
    public event EventHandler TrezorDisconnected;
    #endregion

    #region Public Properties
    public IDevice TrezorDevice { get; private set; }
    public  DeviceListener DeviceListener { get;  }
    #endregion

    #region Constructor
    public TrezorExample()
    {
        DeviceListener = new DeviceListener(_DeviceDefinitions, PollMilliseconds) { Logger = new DebugLogger() };
    }
    #endregion

    #region Event Handlers
    private void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
    {
        TrezorDevice = e.Device;
        TrezorInitialized?.Invoke(this, new EventArgs());
    }

    private void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
    {
        TrezorDevice = null;
        TrezorDisconnected?.Invoke(this, new EventArgs());
    }
    #endregion

    #region Public Methods
    public void StartListening()
    {
        TrezorDevice?.Close();
        DeviceListener.DeviceDisconnected += DevicePoller_DeviceDisconnected;
        DeviceListener.DeviceInitialized += DevicePoller_DeviceInitialized;
        DeviceListener.Start();
    }

    public async Task InitializeTrezorAsync()
    {
        //Get the first available device and connect to it
        var devices = await DeviceManager.Current.GetDevicesAsync(_DeviceDefinitions);
        TrezorDevice = devices.FirstOrDefault();

        if (TrezorDevice == null) throw new Exception("There were no devices found");

        await TrezorDevice.InitializeAsync();
    }

    public async Task<byte[]> WriteAndReadFromDeviceAsync()
    {
        //Create a buffer with 3 bytes (initialize)
        var writeBuffer = new byte[64];
        writeBuffer[0] = 0x3f;
        writeBuffer[1] = 0x23;
        writeBuffer[2] = 0x23;

        //Write the data to the device
        return await TrezorDevice.WriteAndReadAsync(writeBuffer);
    }

    public void Dispose()
    {
        DeviceListener.DeviceDisconnected -= DevicePoller_DeviceDisconnected;
        DeviceListener.DeviceInitialized -= DevicePoller_DeviceInitialized;
        DeviceListener.Dispose();
        TrezorDevice?.Dispose();
    }
    #endregion
}
```
[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/master/src/Usb.Net.UWP.Sample/TrezorExample.cs)

