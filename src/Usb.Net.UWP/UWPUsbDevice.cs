using Device.Net;
using Device.Net.UWP;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Usb;
using Windows.Foundation;

namespace Usb.Net.UWP
{
    public class UWPUsbDevice : UWPDeviceBase<UsbDevice>
    {
        #region Fields
        /// <summary>
        /// TODO: It should be possible to select a different configuration/interface
        /// </summary>
        private UsbInterface _DefaultConfigurationInterface;
        private UsbInterruptOutPipe _DefaultOutPipe;
        private UsbInterruptInPipe _DefaultInPipe;
        #endregion

        #region Public Override Properties
        public override ushort WriteBufferSize => (ushort)_DefaultOutPipe.EndpointDescriptor.MaxPacketSize;
        public override ushort ReadBufferSize => (ushort)_DefaultInPipe.EndpointDescriptor.MaxPacketSize;
        #endregion

        #region Constructors
        public UWPUsbDevice() : base()
        {
        }

        public UWPUsbDevice(ConnectedDeviceDefinition deviceDefinition) : base(deviceDefinition.DeviceId)
        {
            DeviceDefinition = deviceDefinition;
        }
        #endregion

        #region Private Methods
        public override async Task InitializeAsync()
        {
            await GetDevice(DeviceId);

            if (_ConnectedDevice != null)
            {
                var usbInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();

                if (usbInterface == null)
                {
                    _ConnectedDevice.Dispose();
                    throw new Exception("There was no Usb Interface found for the device.");
                }

                var interruptPipe = usbInterface.InterruptInPipes.FirstOrDefault();

                if (interruptPipe == null)
                {
                    throw new Exception("There was no interrupt pipe found on the interface");
                }

                interruptPipe.DataReceived += InterruptPipe_DataReceived;

                //TODO: Fill in the DeviceDefinition...

                // TODO: It should be possible to select a different configurations, interface, and pipes

                _DefaultConfigurationInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();

                //TODO: Clean up this messaging and move down to a base class across platforms
                if (_DefaultConfigurationInterface == null) throw new Exception("Could not get the default interface configuration for the USB device");

                _DefaultOutPipe = _DefaultConfigurationInterface.InterruptOutPipes.FirstOrDefault();

                if (_DefaultOutPipe == null) throw new Exception("Could not get the default out pipe for the default USB interface");

                _DefaultInPipe = _DefaultConfigurationInterface.InterruptInPipes.FirstOrDefault();

                if (_DefaultOutPipe == null) throw new Exception("Could not get the default in pipe for the default USB interface");
            }
            else
            {
                throw new Exception($"Could not connect to device with Device Id {DeviceId}. Check that the package manifest has been configured to allow this device.");
            }
        }

        protected override IAsyncOperation<UsbDevice> FromIdAsync(string id)
        {
            return UsbDevice.FromIdAsync(id);
        }

        #endregion

        #region Event Handlers
        private void InterruptPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            HandleDataReceived(args.InterruptData.ToArray());
        }
        #endregion

        #region Public Methods
        public override async Task WriteAsync(byte[] bytes)
        {
            if (_DefaultOutPipe == null) throw new Exception("The device has not been initialized.");

            if (bytes.Length > WriteBufferSize) throw new Exception("The buffer size is too large");
            await _DefaultOutPipe.OutputStream.WriteAsync(bytes.AsBuffer());

            Tracer?.Trace(false, bytes);
        }
        #endregion
    }
}
