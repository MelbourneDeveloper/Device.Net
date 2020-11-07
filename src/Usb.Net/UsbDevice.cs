using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
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
        public UsbDevice(
            string deviceId,
            IUsbInterfaceManager usbInterfaceManager) : this(deviceId, usbInterfaceManager, null)
        {
        }

        public UsbDevice(
            string deviceId,
            IUsbInterfaceManager usbInterfaceManager,
            ILoggerFactory loggerFactory = null) : base(
                deviceId,
                loggerFactory,
                (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<UsbDevice>()) => UsbInterfaceManager = usbInterfaceManager ?? throw new ArgumentNullException(nameof(usbInterfaceManager));
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        public async Task InitializeAsync()
        {
            await UsbInterfaceManager.InitializeAsync();
            ConnectedDeviceDefinition = await UsbInterfaceManager.GetConnectedDeviceDefinitionAsync();
        }

        public override async Task<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            return UsbInterfaceManager.ReadUsbInterface == null
                ? throw new DeviceException(Messages.ErrorMessageNoReadInterfaceSpecified)
                : await UsbInterfaceManager.ReadUsbInterface.ReadAsync(ReadBufferSize, cancellationToken);
        }

        public override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            return UsbInterfaceManager.WriteUsbInterface == null
                ? throw new DeviceException(Messages.ErrorMessageNoWriteInterfaceSpecified)
                : UsbInterfaceManager.WriteUsbInterface.WriteAsync(data, cancellationToken);
        }

        //TODO: public Task<uint> SendControlOutTransferAsync(UsbSetupPacket setupPacket, IBuffer buffer);
        public uint SendControlOutTransfer(ISetupPacket setupPacket, byte[] buffer) => UsbInterfaceManager.SendControlOutTransfer(setupPacket, buffer);

        //TODO: public Task<uint> SendControlInTransferAsync(UsbSetupPacket setupPacket);
        public uint SendControlInTransfer(ISetupPacket setupPacket) => UsbInterfaceManager.SendControlInTransfer(setupPacket);

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
