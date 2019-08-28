using Device.Net;
using Device.Net.Exceptions;
using System;
using System.Threading.Tasks;

namespace Usb.Net
{
    public class UsbDevice : DeviceBase, IUsbDevice
    {
        #region Fields
        private bool disposed;
        private bool _IsClosing;
        #endregion

        #region Public Overrride Properties
        public override bool IsInitialized => UsbInterfaceManager.IsInitialized;
        public IUsbInterfaceManager UsbInterfaceManager { get; }
        public override ushort WriteBufferSize => UsbInterfaceManager.WriteBufferSize;
        public override ushort ReadBufferSize => UsbInterfaceManager.ReadBufferSize;
        #endregion

        #region Constructor
        /// <summary>
        /// TODO: Remove the tracer from the constructor. This will get passed to the handler so there's no need for it on the device itself.
        /// </summary>
        public UsbDevice(string deviceId, IUsbInterfaceManager usbInterfaceManager, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
            UsbInterfaceManager = usbInterfaceManager ?? throw new ArgumentNullException(nameof(usbInterfaceManager));
        }
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        public async Task InitializeAsync()
        {
            await UsbInterfaceManager.InitializeAsync();
            ConnectedDeviceDefinition = await UsbInterfaceManager.GetConnectedDeviceDefinitionAsync();
        }

        public override async Task<ReadResult> ReadAsync()
        {
            if (UsbInterfaceManager.ReadUsbInterface == null) throw new DeviceException(Messages.ErrorMessageNoReadInterfaceSpecified);

            return await UsbInterfaceManager.ReadUsbInterface.ReadAsync(ReadBufferSize);
        }

        public override Task WriteAsync(byte[] data)
        {
            if (UsbInterfaceManager.WriteUsbInterface == null) throw new DeviceException(Messages.ErrorMessageNoWriteInterfaceSpecified);

            return UsbInterfaceManager.WriteUsbInterface.WriteAsync(data);
        }

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            try
            {
                UsbInterfaceManager?.Close();
            }
            catch (Exception)
            {
                //TODO: Logging
            }

            _IsClosing = false;
        }

        public sealed override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Close();

            base.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Finalizer
        ~UsbDevice()
        {
            Dispose();
        }
        #endregion
    }
}
