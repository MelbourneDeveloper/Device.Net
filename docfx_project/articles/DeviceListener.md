Note: _DeviceListener is no longer the recommended approach for event-driven connection/disconnection. `DeviceManager` is a work in progress and should be used where possible. See the documentation.

See [code reference](https://melbournedeveloper.github.io/Device.Net/api/Device.Net.DeviceListener.html)

A device listener is a platform-independent class that will handle connecting and disconnecting from devices that connect to the computer. Firstly, you must specify the filter definitions for the devices you want to connect to, and create the factories. This works the same way across each platform, but you must register the platform-specific device factories to make this work.

Here is an example for registering both factories on Windows:

```cs
_trezorFactories = new List<IDeviceFactory>
{
    TrezorExample.UsbDeviceDefinitions.CreateWindowsUsbDeviceFactory(_loggerFactory),
    TrezorExample.HidDeviceDefinitions.CreateWindowsHidDeviceFactory(_loggerFactory),
}.Aggregate(_loggerFactory);
```

_Note: if you have not already been through the process you will need to [configure device permissions](https://melbournedeveloper.github.io/Device.Net/articles/DevicePermissionSetup.html) on Android, or UWP._

Here is a working example that listens for a device and notifies you when the device is connected or disconnected. All three sample applications (Android, UWP, Windows, and LibUsb) use this same code.

```cs
using Device.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Sample
{
    internal sealed class TrezorExample : IDisposable
    {
        #region Fields
        private const int PollMilliseconds = 6000;

        //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
        public static readonly List<FilterDeviceDefinition> HidDeviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition( vendorId: 0x534C, productId:0x0001, label:"Trezor One Firmware 1.6.x", usagePage:65280 )
        };

        public static readonly List<FilterDeviceDefinition> UsbDeviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition( vendorId: 0x534C, productId:0x0001, label:"Trezor One Firmware 1.6.x (Android Only)" ),
            new FilterDeviceDefinition( vendorId: 0x1209, productId:0x53C1, label:"Trezor One Firmware 1.7.x" ),
            new FilterDeviceDefinition( vendorId: 0x1209, productId:0x53C0, label:"Model T" )
        };
        #endregion

        #region Events
        public event EventHandler TrezorInitialized;
        public event EventHandler TrezorDisconnected;
        #endregion

        #region Public Properties
        public IDevice TrezorDevice { get; private set; }
        public IDeviceFactory DeviceManager { get; }
        public DeviceListener DeviceListener { get; }
        #endregion

        #region Constructor
        public TrezorExample(IDeviceFactory deviceManager, ILoggerFactory loggerFactory)
        {
            DeviceManager = deviceManager;
            DeviceListener = new DeviceListener(deviceManager, PollMilliseconds, loggerFactory);
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
            var devices = await DeviceListener.DeviceFactory.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false);
            var firstConnectedDeviceDefinition = devices.FirstOrDefault();
            TrezorDevice = await DeviceListener.DeviceFactory.GetDeviceAsync(firstConnectedDeviceDefinition).ConfigureAwait(false);

            if (TrezorDevice == null) throw new Exception("There were no devices found");

            await TrezorDevice.InitializeAsync().ConfigureAwait(false);
        }

        public async Task<byte[]> WriteAndReadFromDeviceAsync()
        {
            //Create a buffer with 3 bytes (initialize)
            var writeBuffer = new byte[64];
            writeBuffer[0] = 0x3f;
            writeBuffer[1] = 0x23;
            writeBuffer[2] = 0x23;

            //Write the data to the device
            return await TrezorDevice.WriteAndReadAsync(writeBuffer).ConfigureAwait(false);
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
}
```
[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/087566e2c0f4dc11a0b9f2177a9a487efbcf181f/src/Usb.Net.UWP.Sample/TrezorExample.cs#L10)