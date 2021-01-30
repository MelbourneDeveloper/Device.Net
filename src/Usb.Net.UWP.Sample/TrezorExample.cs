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
