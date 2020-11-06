using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using usbDevice = Android.Hardware.Usb.UsbDevice;

namespace Usb.Net.Android
{
    public class AndroidUsbInterfaceManager : UsbInterfaceManager, IUsbInterfaceManager
    {
        #region Fields
        private UsbDeviceConnection _UsbDeviceConnection;
        private usbDevice _UsbDevice;
        private readonly SemaphoreSlim _InitializingSemaphoreSlim = new SemaphoreSlim(1, 1);
        private bool _IsClosing;
        private bool disposed;
        private ushort? ReadBufferSizeProtected { get; set; }
        private ushort? WriteBufferSizeProtected { get; set; }

        #endregion

        #region Public Override Properties
        public bool IsInitialized => _UsbDeviceConnection != null;
        #endregion

        #region Public Properties
        public UsbManager UsbManager { get; }
        public Context AndroidContext { get; private set; }
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
            Context androidContext,
            int deviceNumberId,
            ILoggerFactory loggerFactory = null,
            ushort? readBufferLength = null,
            ushort? writeBufferLength = null) : base(loggerFactory)
        {
            ReadBufferSizeProtected = readBufferLength;
            WriteBufferSizeProtected = writeBufferLength;
            UsbManager = usbManager ?? throw new ArgumentNullException(nameof(usbManager));
            AndroidContext = androidContext ?? throw new ArgumentNullException(nameof(androidContext));
            DeviceNumberId = deviceNumberId;

            Logger.LogInformation("read buffer size: {readBufferLength} writeBufferLength {writeBufferLength}", readBufferLength, writeBufferLength);
        }
        #endregion

        #region Public Methods 

        public override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Close();

            _InitializingSemaphoreSlim.Dispose();

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            try
            {
                _UsbDeviceConnection?.Dispose();
                _UsbDevice?.Dispose();
                ReadUsbInterface?.Dispose();
                WriteUsbInterface?.Dispose();

                _UsbDeviceConnection = null;
                _UsbDevice = null;
                ReadUsbInterface = null;
                WriteUsbInterface = null;
            }
            catch (Exception)
            {
                //TODO: Logging
            }

            _IsClosing = false;
        }

        public Task<ReadResult> ReadAsync() => ReadUsbInterface.ReadAsync(ReadBufferSize);

        public Task WriteAsync(byte[] data) => WriteUsbInterface.WriteAsync(data);

        #endregion

        #region Private  Methods
        private Task<bool?> RequestPermissionAsync()
        {
            Logger.LogInformation("Requesting USB permission");

            var taskCompletionSource = new TaskCompletionSource<bool?>();

            var usbPermissionBroadcastReceiver = new UsbPermissionBroadcastReceiver(
                UsbManager,
                _UsbDevice,
                AndroidContext,
                LoggerFactory.CreateLogger<UsbPermissionBroadcastReceiver>());
            usbPermissionBroadcastReceiver.Received += (sender, eventArgs) =>
            {
                taskCompletionSource.SetResult(usbPermissionBroadcastReceiver.IsPermissionGranted);
            };

            usbPermissionBroadcastReceiver.Register();

            return taskCompletionSource.Task;
        }

        public async Task InitializeAsync()
        {
            using var logScope = Logger.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceNumberId, nameof(InitializeAsync));

            try
            {

                if (disposed) throw new DeviceException(Messages.DeviceDisposedErrorMessage);

                await _InitializingSemaphoreSlim.WaitAsync();

                Close();

                _UsbDevice = UsbManager.DeviceList.Select(d => d.Value).FirstOrDefault(d => d.DeviceId == DeviceNumberId);

                if (_UsbDevice == null)
                {
                    throw new DeviceException($"The device {DeviceNumberId} is not connected to the system");
                }

                Logger.LogInformation("Found device: {deviceName} Id: {deviceId}", _UsbDevice.DeviceName, _UsbDevice.DeviceId);

                var isPermissionGranted = await RequestPermissionAsync();
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

                    var androidUsbInterface = new AndroidUsbInterface(usbInterface, _UsbDeviceConnection, LoggerFactory.CreateLogger<AndroidUsbInterface>(), ReadBufferSizeProtected, WriteBufferSizeProtected);

                    Logger.LogInformation("Interface found. Id: {id} Endpoint Count: {endpointCount} Interface Class: {interfaceclass} Interface Subclass: {interfacesubclass} Name: {name}", usbInterface.Id, usbInterface.EndpointCount, usbInterface.InterfaceClass, usbInterface.InterfaceSubclass, usbInterface.Name);

                    UsbInterfaces.Add(androidUsbInterface);

                    for (var endpointNumber = 0; endpointNumber < usbInterface.EndpointCount; endpointNumber++)
                    {
                        var usbEndpoint = usbInterface.GetEndpoint(endpointNumber);

                        if (usbEndpoint != null)
                        {
                            var androidUsbEndpoint = new AndroidUsbEndpoint(usbEndpoint, interfaceNumber, LoggerFactory.CreateLogger<AndroidUsbEndpoint>());
                            androidUsbInterface.UsbInterfaceEndpoints.Add(androidUsbEndpoint);
                        }
                    }

                    await androidUsbInterface.ClaimInterface();
                }

                RegisterDefaultInterfaces();

                Logger.LogInformation("Device initialized successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing device");
                throw;
            }
            finally
            {
                _InitializingSemaphoreSlim.Release();
            }
        }

        public static ConnectedDeviceDefinition GetAndroidDeviceDefinition(usbDevice usbDevice)
        {
            if (usbDevice == null) throw new ArgumentNullException(nameof(usbDevice));

            var deviceId = usbDevice.DeviceId.ToString(Helpers.ParsingCulture);

            return new ConnectedDeviceDefinition(
                deviceId,
                DeviceType.Usb,
                //productName: usbDevice.ProductName,
                //manufacturer: usbDevice.ManufacturerName,
                //serialNumber: usbDevice.SerialNumber,
                productId: (uint)usbDevice.ProductId,
                vendorId: (uint)usbDevice.VendorId
            );
        }

        public Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync() => Task.Run(() => GetAndroidDeviceDefinition(_UsbDevice));
        #endregion

        #region Finalizer
        /// <summary>
        /// What's this then?
        /// </summary>
        ~AndroidUsbInterfaceManager()
        {
            Dispose();
        }
        #endregion
    }
}