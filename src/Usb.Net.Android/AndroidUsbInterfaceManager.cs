using Android.Hardware.Usb;
using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using usbDevice = Android.Hardware.Usb.UsbDevice;

#nullable enable

namespace Usb.Net.Android
{
    /// <summary>
    ///<inheritdoc cref="IUsbInterfaceManager"/>
    /// </summary>
    public class AndroidUsbInterfaceManager : UsbInterfaceManager, IUsbInterfaceManager
    {
        #region Fields
        private UsbDeviceConnection _UsbDeviceConnection;
        private usbDevice _UsbDevice;
        private readonly SemaphoreSlim _InitializingSemaphoreSlim = new(1, 1);
        private bool disposed;
        private ushort? ReadBufferSizeProtected { get; set; }
        private ushort? WriteBufferSizeProtected { get; set; }
        private readonly IAndroidFactory _androidFactory;
        private readonly Func<usbDevice, IUsbPermissionBroadcastReceiver> _getUsbPermissionBroadcastReceiver;
        #endregion

        #region Public Override Properties
        public bool IsInitialized => _UsbDeviceConnection != null;
        #endregion

        #region Public Properties
        public UsbManager UsbManager { get; }
        public ushort WriteBufferSize => WriteBufferSizeProtected == null && WriteUsbInterface == null
                    ? throw new InvalidOperationException("WriteBufferSize was not specified, and no write usb interface has been selected")
                    : WriteBufferSizeProtected ?? WriteUsbInterface.ReadBufferSize;

        public ushort ReadBufferSize => ReadBufferSizeProtected == null && ReadUsbInterface == null
                    ? throw new InvalidOperationException("ReadBufferSize was not specified, and no read usb interface has been selected")
                    : ReadBufferSizeProtected ?? ReadUsbInterface.ReadBufferSize;

        public int DeviceNumberId { get; }
        #endregion

        #region Constructor
        public AndroidUsbInterfaceManager(
            UsbManager usbManager,
            int deviceNumberId,
            IAndroidFactory androidFactory,
            Func<usbDevice, IUsbPermissionBroadcastReceiver> usbPermissionBroadcastReceiver,
            ILoggerFactory? loggerFactory = null,
            ushort? readBufferLength = null,
            ushort? writeBufferLength = null) : base(loggerFactory)
        {
            ReadBufferSizeProtected = readBufferLength;
            WriteBufferSizeProtected = writeBufferLength;
            UsbManager = usbManager ?? throw new ArgumentNullException(nameof(usbManager));
            DeviceNumberId = deviceNumberId;
            _androidFactory = androidFactory;
            _getUsbPermissionBroadcastReceiver = usbPermissionBroadcastReceiver;

            Logger.LogInformation("read buffer size: {readBufferLength} writeBufferLength {writeBufferLength}", readBufferLength, writeBufferLength);
        }
        #endregion

        #region Public Methods 

        public sealed override void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, DeviceNumberId);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, DeviceNumberId);

            Close();

            try
            {
                _UsbDeviceConnection?.Dispose();
                _UsbDevice?.Dispose();
                ReadUsbInterface?.Dispose();
                WriteUsbInterface?.Dispose();

                ReadUsbInterface = null;
                WriteUsbInterface = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Dispose error DeviceId: {deviceNumberId}", DeviceNumberId);
            }

            _InitializingSemaphoreSlim.Dispose();

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        public override void Close()
        {
            _UsbDeviceConnection?.Close();
            base.Close();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (disposed) throw new DeviceException(Messages.DeviceDisposedErrorMessage);

            using var logScope = Logger.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceNumberId, nameof(InitializeAsync));

            if (IsInitialized) Logger.LogWarning("Device is already initialized...");

            Logger.LogInformation("Attempting to initialize...");

            try
            {
                await Task.Run(async () =>
                {
                    Logger.LogTrace("Waiting for initialization lock ... {deviceId}", DeviceNumberId);

                    await _InitializingSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                    Close();

                    _UsbDevice = UsbManager.DeviceList.Select(d => d.Value).FirstOrDefault(d => d.DeviceId == DeviceNumberId);

                    if (_UsbDevice == null)
                    {
                        throw new DeviceException($"The device {DeviceNumberId} is not connected to the system");
                    }

                    Logger.LogInformation("Found device: {deviceName} Id: {deviceId}", _UsbDevice.DeviceName, _UsbDevice.DeviceId);

                    var isPermissionGranted = await RequestPermissionAsync().ConfigureAwait(false);
                    if (!isPermissionGranted.HasValue)
                    {
                        throw new DeviceException("User did not respond to permission request");
                    }

                    if (!isPermissionGranted.Value)
                    {
                        throw new DeviceException("The user did not give the permission to access the device");
                    }

                    _UsbDeviceConnection = UsbManager.OpenDevice(_UsbDevice);

                    if (_UsbDeviceConnection == null)
                    {
                        throw new DeviceException("could not open connection");
                    }

                    Logger.LogInformation("Interface count: {count}", _UsbDevice.InterfaceCount);

                    for (var interfaceNumber = 0; interfaceNumber < _UsbDevice.InterfaceCount; interfaceNumber++)
                    {
                        //TODO: This is the default interface but other interfaces might be needed so this needs to be changed.
                        var usbInterface = _UsbDevice.GetInterface(interfaceNumber);

                        var androidUsbInterface = new AndroidUsbInterface(
                            usbInterface,
                            _UsbDeviceConnection,
                            _androidFactory,
                            LoggerFactory.CreateLogger<AndroidUsbInterface>(),
                            ReadBufferSizeProtected,
                            WriteBufferSizeProtected);

                        Logger.LogInformation("Interface found. Id: {id} Endpoint Count: {endpointCount} Interface Class: {interfaceclass} Interface Subclass: {interfacesubclass} Name: {name}", usbInterface.Id, usbInterface.EndpointCount, usbInterface.InterfaceClass, usbInterface.InterfaceSubclass, usbInterface.Name);

                        UsbInterfaces.Add(androidUsbInterface);

                        for (var endpointNumber = 0; endpointNumber < usbInterface.EndpointCount; endpointNumber++)
                        {
                            var usbEndpoint = usbInterface.GetEndpoint(endpointNumber);

                            if (usbEndpoint == null) continue;
                            var androidUsbEndpoint = new AndroidUsbEndpoint(usbEndpoint, interfaceNumber, LoggerFactory.CreateLogger<AndroidUsbEndpoint>());
                            androidUsbInterface.UsbInterfaceEndpoints.Add(androidUsbEndpoint);
                        }

                        await androidUsbInterface.ClaimInterface().ConfigureAwait(false);
                    }

                    RegisterDefaultInterfaces();

                    Logger.LogInformation("Device initialized successfully.");
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing device");
                throw;
            }
            finally
            {
                Logger.LogTrace("Releasing initialization lock");
                _ = _InitializingSemaphoreSlim.Release();
            }
        }

        public static ConnectedDeviceDefinition GetAndroidDeviceDefinition(usbDevice usbDevice)
        {
            if (usbDevice == null) throw new ArgumentNullException(nameof(usbDevice));

            var deviceId = usbDevice.DeviceId.ToString(AndroidUsbFactoryExtensions.IntParsingCulture);

            return new ConnectedDeviceDefinition(
                deviceId,
                DeviceType.Usb,
                //TODO: Put these back when it is safe to do so
                //productName: usbDevice.ProductName,
                //manufacturer: usbDevice.ManufacturerName,
                //serialNumber: usbDevice.SerialNumber,
                productId: (uint)usbDevice.ProductId,
                vendorId: (uint)usbDevice.VendorId
            );
        }

        public Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync(CancellationToken cancellationToken = default) => Task.Run(() => GetAndroidDeviceDefinition(_UsbDevice), cancellationToken);
        #endregion

        #region Private  Methods
        private Task<bool?> RequestPermissionAsync()
        {
            Logger.LogInformation("Requesting USB permission");

            var taskCompletionSource = new TaskCompletionSource<bool?>();

            var usbPermissionBroadcastReceiver = _getUsbPermissionBroadcastReceiver(_UsbDevice);

            usbPermissionBroadcastReceiver.Received += (sender, eventArgs) => taskCompletionSource.SetResult(usbPermissionBroadcastReceiver.IsPermissionGranted);

            usbPermissionBroadcastReceiver.Register();

            return taskCompletionSource.Task;
        }
        #endregion
    }
}