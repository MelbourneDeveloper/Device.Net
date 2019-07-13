using Device.Net;
using Device.Net.Exceptions;
using Device.Net.UWP;
using System;
using System.Collections.Generic;
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
        #region Public Properties
        public UsbInterfaceHandler UsbInterfaceHandler { get; }
        #endregion

        #region Public Override Properties
        public override ushort WriteBufferSize => WriteUsbInterface.WriteEndpoint.WriteBufferSize;
        public override ushort ReadBufferSize => ReadUsbInterface.ReadEndpoint.ReadBufferSize;

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

        public UWPUsbDeviceHandler(ConnectedDeviceDefinition connectedDeviceDefinition, ILogger logger, ITracer tracer) : base(connectedDeviceDefinition?.DeviceId, logger, tracer)
        {
            ConnectedDeviceDefinition = connectedDeviceDefinition ?? throw new ArgumentNullException(nameof(connectedDeviceDefinition));
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
                if (ConnectedDevice.Configuration.UsbInterfaces == null)
                {
                    ConnectedDevice.Dispose();
                    throw new DeviceException(Messages.ErrorMessageNoInterfaceFound);
                }

                var interfaceIndex = 0;
                foreach (var usbInterface in ConnectedDevice.Configuration.UsbInterfaces)
                {
                    UsbInterfaceHandler.UsbInterfaces.Add(new UWPUsbInterface(usbInterface, Logger, Tracer));

                    if (usbInterface.InterruptInPipes.Count==0)
                    {
                        Log(Messages.MessageNoEndpointFound + $" Interface index: {interfaceIndex}", null);
                        continue;
                    } 
                    interfaceIndex++;
                }
            }
            else
            {
                throw new DeviceException(Messages.GetErrorMessageCantConnect(DeviceId));
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
        public void Dispose()
        {


            UsbInterfaceHandler.Dispose();
        }

        public Task WriteAsync(byte[] data)
        {
            return WriteUsbInterface.WriteAsync(data);
        }

        public Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync()
        {
            return Task.FromResult(ConnectedDeviceDefinition);
        }
        #endregion
    }
}
