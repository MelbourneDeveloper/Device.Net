using Device.Net;
using Device.Net.Exceptions;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using usbControlRequestType = Windows.Devices.Usb.UsbControlRequestType;
using usbControlTransferType = Windows.Devices.Usb.UsbControlTransferType;
using usbSetupPacket = Windows.Devices.Usb.UsbSetupPacket;
using windowsUsbDevice = Windows.Devices.Usb.UsbDevice;

namespace Usb.Net.UWP
{
    public class UWPUsbInterfaceManager : UWPDeviceBase<windowsUsbDevice>, IUsbInterfaceManager
    {
        #region Fields
        private bool disposed;
        private readonly ushort? _WriteBufferSize;
        private readonly ushort? _ReadBufferSize;
        private readonly Func<windowsUsbDevice, SetupPacket, byte[], CancellationToken, Task<TransferResult>> _performControlTransferAsync;
        #endregion

        #region Public Properties
        public UsbInterfaceManager UsbInterfaceHandler { get; }
        #endregion

        #region Public Override Properties
        public ushort WriteBufferSize => _WriteBufferSize ?? WriteUsbInterface.WriteBufferSize;
        public ushort ReadBufferSize => _ReadBufferSize ?? ReadUsbInterface.WriteBufferSize;

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
        public UWPUsbInterfaceManager(
            ConnectedDeviceDefinition connectedDeviceDefinition,
            IDataReceiver dataReceiver = null,
            Func<windowsUsbDevice, SetupPacket, byte[], CancellationToken, Task<TransferResult>> performControlTransferAsync = null,
            ILoggerFactory loggerFactory = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null) :
            base(
                connectedDeviceDefinition?.DeviceId,
                dataReceiver ??
                new UWPDataReceiver(
                    new Observable<TransferResult>(),
                    (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<UWPDataReceiver>()), loggerFactory ?? NullLoggerFactory.Instance)
        {
            ConnectedDeviceDefinition = connectedDeviceDefinition ?? throw new ArgumentNullException(nameof(connectedDeviceDefinition));
            UsbInterfaceHandler = new UsbInterfaceManager(loggerFactory);
            _WriteBufferSize = writeBufferSize;
            _ReadBufferSize = readBufferSize;
            _performControlTransferAsync = performControlTransferAsync ?? PerformControlTransferAsync;
        }
        #endregion

        #region Public Methods
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

            const string message = "Initialising {deviceId}";

            using var scope = Logger.BeginScope(message, DeviceId);
            Logger.LogInformation(message, DeviceId);

            await GetDeviceAsync(DeviceId, cancellationToken);

            if (ConnectedDevice != null)
            {
                Logger.LogInformation("Got device {deviceId}", DeviceId);

                if (ConnectedDevice.Configuration.UsbInterfaces == null || ConnectedDevice.Configuration.UsbInterfaces.Count == 0)
                {
                    ConnectedDevice.Dispose();
                    throw new DeviceException(Messages.ErrorMessageNoInterfaceFound);
                }

                var interfaceIndex = 0;
                foreach (var usbInterface in ConnectedDevice.Configuration.UsbInterfaces)
                {
                    var uwpUsbInterface = new UWPUsbInterface(
                        usbInterface,
                        (sp, data, c) => _performControlTransferAsync(ConnectedDevice, sp, data, c),
                        DataReceiver,
                        LoggerFactory,
                        _ReadBufferSize,
                        _WriteBufferSize);

                    UsbInterfaceHandler.UsbInterfaces.Add(uwpUsbInterface);
                    interfaceIndex++;
                }
            }
            else
            {
                var deviceException = new DeviceException(Messages.GetErrorMessageCantConnect(DeviceId));
                Logger.LogError(deviceException, "Error getting device");
                throw deviceException;
            }

            UsbInterfaceHandler.RegisterDefaultInterfaces();
        }

        public override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            UsbInterfaceHandler?.Dispose();
            base.Dispose();
        }

        public Task WriteAsync(byte[] data) => WriteUsbInterface.WriteAsync(data);

        public Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync(CancellationToken cancellationToken = default) => Task.FromResult(ConnectedDeviceDefinition);

        public Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default) => ReadUsbInterface.ReadAsync(ReadBufferSize, cancellationToken);
        #endregion

        #region Private Methods
        private static async Task<TransferResult> PerformControlTransferAsync(windowsUsbDevice ConnectedDevice, SetupPacket setupPacket, byte[] buffer, CancellationToken cancellationToken = default)
        {
            var uwpSetupPacket = new usbSetupPacket
            {
                Index = setupPacket.Index,
                Length = setupPacket.Length,
                Request = setupPacket.Request,
                RequestType = new usbControlRequestType
                {
                    ControlTransferType = setupPacket.RequestType.Type switch
                    {
                        RequestType.Standard => usbControlTransferType.Standard,
                        RequestType.Class => usbControlTransferType.Class,
                        RequestType.Vendor => usbControlTransferType.Vendor,
                        _ => throw new NotImplementedException()
                    }
                },
                Value = setupPacket.Value
            };

            if (setupPacket.RequestType.Direction == RequestDirection.In)
            {
                //Read
                var readBuffer = await ConnectedDevice.SendControlInTransferAsync(uwpSetupPacket);

                return new TransferResult(readBuffer.ToArray(), readBuffer.Length);
            }
            else
            {
                //Write
                var bytesWritten = await ConnectedDevice.SendControlOutTransferAsync(uwpSetupPacket);
                return new TransferResult(buffer, bytesWritten);
            }
        }
        #endregion

        #region Protected Methods
        protected override IAsyncOperation<windowsUsbDevice> FromIdAsync(string id) => windowsUsbDevice.FromIdAsync(id);
        #endregion
    }
}
