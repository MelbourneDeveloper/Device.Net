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
        public override bool IsInitialized => UsbDeviceHandler.IsInitialized;
        public IUsbDeviceHandler UsbDeviceHandler { get; }
        public override ushort WriteBufferSize => UsbDeviceHandler.WriteBufferSize;
        public override ushort ReadBufferSize => UsbDeviceHandler.ReadBufferSize;
        #endregion

        #region Constructor
        /// <summary>
        /// TODO: Remove the tracer from the constructor. This will get passed to the handler so there's no need for it on the device itself.
        /// </summary>
        public UsbDevice(IUsbDeviceHandler usbDeviceHandler, ILogger logger, ITracer tracer) : base(logger, tracer)
        {
            UsbDeviceHandler = usbDeviceHandler ?? throw new ArgumentNullException(nameof(usbDeviceHandler));
        }
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        public async Task InitializeAsync()
        {
            await UsbDeviceHandler.InitializeAsync();
            ConnectedDeviceDefinition = await UsbDeviceHandler.GetConnectedDeviceDefinitionAsync();
        }

        public override Task<byte[]> ReadAsync()
        {
            if (UsbDeviceHandler.ReadUsbInterface == null) throw new DeviceException(Messages.ErrorMessageNoReadInterfaceSpecified);

            return UsbDeviceHandler.ReadUsbInterface.ReadAsync(ReadBufferSize);
        }

        public override Task WriteAsync(byte[] data)
        {
            if (UsbDeviceHandler.WriteUsbInterface == null) throw new DeviceException(Messages.ErrorMessageNoWriteInterfaceSpecified);

            return UsbDeviceHandler.WriteUsbInterface.WriteAsync(data);
        }

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            try
            {
                UsbDeviceHandler?.Close();
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
