using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    ///<inheritdoc cref="IUsbDevice"/>
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
        public ConnectedDeviceDefinition ConnectedDeviceDefinition { get; private set; }
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
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await UsbInterfaceManager.InitializeAsync(cancellationToken).ConfigureAwait(false);
            ConnectedDeviceDefinition = await UsbInterfaceManager.GetConnectedDeviceDefinitionAsync(cancellationToken).ConfigureAwait(false);
        }

        public override Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default) =>
            UsbInterfaceManager.ReadUsbInterface == null
                ? throw new DeviceException(Messages.ErrorMessageNoReadInterfaceSpecified)
                : UsbInterfaceManager.ReadUsbInterface.ReadAsync(ReadBufferSize, cancellationToken);

        public override Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default) =>
            UsbInterfaceManager.WriteUsbInterface == null
                ? throw new DeviceException(Messages.ErrorMessageNoWriteInterfaceSpecified)
                : UsbInterfaceManager.WriteUsbInterface.WriteAsync(data, cancellationToken);

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            Logger.LogInformation("Closing device ... {deviceId}", DeviceId);

            try
            {
                //TODO: The manager needs to be thrown away and recreated. This is probably ok because it should be done as part of initialization
                //However not sure if maybe this method should be called Close?
                UsbInterfaceManager.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error disposing");
            }

            _IsClosing = false;
        }

        public sealed override void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, DeviceId);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, DeviceId);

            Close();

            base.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
