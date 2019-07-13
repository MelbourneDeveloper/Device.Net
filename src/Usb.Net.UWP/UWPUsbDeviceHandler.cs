using Device.Net;
using Device.Net.Exceptions;
using Device.Net.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Usb;
using Windows.Foundation;
using windowsUsbDevice = Windows.Devices.Usb.UsbDevice;

namespace Usb.Net.UWP
{
    public class UWPUsbDeviceHandler : UWPDeviceHandlerBase<windowsUsbDevice>, IUsbDeviceHandler
    {
        #region Fields
        /// <summary>
        /// TODO: It should be possible to select a different configuration/interface
        /// </summary>
        private UsbInterface _DefaultConfigurationInterface;
        private UsbInterruptOutPipe _DefaultOutPipe;
        private UsbInterruptInPipe _DefaultInPipe;
        #endregion

        #region Public Properties
        public ConnectedDeviceDefinition DeviceDefinition { get; }
        public UsbInterfaceHandler UsbInterfaceHandler { get; }
        #endregion

        #region Public Override Properties
        public override ushort WriteBufferSize => (ushort)_DefaultOutPipe.EndpointDescriptor.MaxPacketSize;
        public override ushort ReadBufferSize => (ushort)_DefaultInPipe.EndpointDescriptor.MaxPacketSize;

        public IUsbInterface ReadUsbInterface
        {
            get => UsbInterfaceHandler.ReadUsbInterface;
            set => UsbInterfaceHandler.ReadUsbInterface = value;
        }

        public IUsbInterface WriteUsbInterface
        {
            get => UsbInterfaceHandler.WriteUsbInterface;
            set => UsbInterfaceHandler.WriteUsbInterface = value;
        }

        public IList<IUsbInterface> UsbInterfaces => UsbInterfaceHandler.UsbInterfaces;
        #endregion

        #region Constructors
        public UWPUsbDeviceHandler(ILogger logger, ITracer tracer) : this(null, logger, tracer)
        {
        }

        public UWPUsbDeviceHandler(ConnectedDeviceDefinition deviceDefinition) : this(deviceDefinition, null, null)
        {
        }

        public UWPUsbDeviceHandler(ConnectedDeviceDefinition deviceDefinition, ILogger logger, ITracer tracer) : base(deviceDefinition.DeviceId, logger, tracer)
        {
            DeviceDefinition = deviceDefinition;
            UsbInterfaceHandler = new UsbInterfaceHandler(logger, tracer);
        }
        #endregion

        #region Private Methods
        public override async Task InitializeAsync()
        {
            if (Disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

            await GetDeviceAsync(DeviceId);

            if (ConnectedDevice != null)
            {
                var usbInterface = ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();

                if (usbInterface == null)
                {
                    ConnectedDevice.Dispose();
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

                _DefaultConfigurationInterface = ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();

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

        protected override IAsyncOperation<windowsUsbDevice> FromIdAsync(string id)
        {
            return windowsUsbDevice.FromIdAsync(id);
        }

        #endregion

        #region Event Handlers
        private void InterruptPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            HandleDataReceived(args.InterruptData.ToArray());
        }
        #endregion

        #region Public Methods
        public async Task WriteAsync(byte[] data)
        {
            if (_DefaultOutPipe == null) throw new Exception(Messages.ErrorMessageNotInitialized);

            if (data.Length > WriteBufferSize) throw new Exception("The buffer size is too large");
            var count = await _DefaultOutPipe.OutputStream.WriteAsync(data.AsBuffer());

            if (count == data.Length)
            {
                Tracer?.Trace(true, data);
            }
            else
            {
                var message = Messages.GetErrorMessageInvalidWriteLength(data.Length, count);
                Logger?.Log(message, GetType().Name, null, LogLevel.Error);
                throw new IOException(message);
            }
        }

        public Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync()
        {
            return Task.FromResult<ConnectedDeviceDefinitionBase>(DeviceDefinition);
        }
        #endregion
    }
}
