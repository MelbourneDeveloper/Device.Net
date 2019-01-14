using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Sample
{
    internal class TrezorExample : IDisposable
    {
        #region Fields
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
        public DeviceListener DeviceListener { get; private set; }
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
            TrezorDevice?.Dispose();
            DeviceListener = new DeviceListener(_DeviceDefinitions, 3000);
            DeviceListener.DeviceDisconnected += DevicePoller_DeviceDisconnected;
            DeviceListener.DeviceInitialized += DevicePoller_DeviceInitialized;
        }

        public async Task InitializeTrezorAsync()
        {
            //Get the first available device and connect to it
            var devices = await DeviceManager.Current.GetDevices(_DeviceDefinitions);
            TrezorDevice = devices.FirstOrDefault();
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
            TrezorDevice?.Dispose();
        }
        #endregion
    }
}
