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
        private readonly ILoggerFactory _loggerFactory;
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
        public ushort WriteBufferSize => WriteBufferSizeProtected ?? WriteUsbInterface.ReadBufferSize;
        public ushort ReadBufferSize => ReadBufferSizeProtected ?? ReadUsbInterface.ReadBufferSize;
        public int DeviceNumberId { get; }
        #endregion

        #region Constructor
        public AndroidUsbInterfaceManager(UsbManager usbManager, Context androidContext, int deviceNumberId, ILoggerFactory loggerFactory, ushort? readBufferLength, ushort? writeBufferLength) : base(loggerFactory.CreateLogger<AndroidUsbInterfaceManager>())
        {
            ReadBufferSizeProtected = readBufferLength;
            WriteBufferSizeProtected = writeBufferLength;
            UsbManager = usbManager ?? throw new ArgumentNullException(nameof(usbManager));
            AndroidContext = androidContext ?? throw new ArgumentNullException(nameof(androidContext));
            DeviceNumberId = deviceNumberId;

            Logger?.LogInformation("read buffer size: {readBufferLength} writeBufferLength {writeBufferLength}", readBufferLength, writeBufferLength);

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
            Logger?.LogInformation("Requesting USB permission");

            var taskCompletionSource = new TaskCompletionSource<bool?>();

            var usbPermissionBroadcastReceiver = new UsbPermissionBroadcastReceiver(UsbManager, _UsbDevice, AndroidContext);
            usbPermissionBroadcastReceiver.Received += (sender, eventArgs) =>
            {
                taskCompletionSource.SetResult(usbPermissionBroadcastReceiver.IsPermissionGranted);
            };

            usbPermissionBroadcastReceiver.Register();

            return taskCompletionSource.Task;
        }

        public async Task InitializeAsync()
        {
            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceNumberId, nameof(InitializeAsync));

                if (disposed) throw new DeviceException(Messages.DeviceDisposedErrorMessage);

                await _InitializingSemaphoreSlim.WaitAsync();

                Close();

                _UsbDevice = UsbManager.DeviceList.Select(d => d.Value).FirstOrDefault(d => d.DeviceId == DeviceNumberId);

                if (_UsbDevice == null)
                {
                    throw new DeviceException($"The device {DeviceNumberId} is not connected to the system");
                }

                Logger?.LogInformation("Found device: {deviceName} Id: {deviceId}", _UsbDevice.DeviceName, _UsbDevice.DeviceId);

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

                for (var x = 0; x < _UsbDevice.InterfaceCount; x++)
                {
                    //TODO: This is the default interface but other interfaces might be needed so this needs to be changed.
                    var usbInterface = _UsbDevice.GetInterface(x);

                    var androidUsbInterface = new AndroidUsbInterface(usbInterface, _UsbDeviceConnection, _loggerFactory.CreateLogger<AndroidUsbInterface>(), ReadBufferSize, WriteBufferSize);

                    Logger.LogInformation("Interface found. Name: {name} Id: {id}", usbInterface.Name, usbInterface.Id);

                    UsbInterfaces.Add(androidUsbInterface);

                    for (var y = 0; y < usbInterface.EndpointCount; y++)
                    {
                        var usbEndpoint = usbInterface.GetEndpoint(y);

                        if (usbEndpoint != null)
                        {
                            //TODO: This is probably all wrong...
                            var androidUsbEndpoint = new AndroidUsbEndpoint(usbEndpoint, _loggerFactory.CreateLogger<AndroidUsbEndpoint>());
                            androidUsbInterface.UsbInterfaceEndpoints.Add(androidUsbEndpoint);
                        }
                    }
                }

                RegisterDefaultInterfaces();

                Logger?.LogInformation("Device initialized successfully.");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error initializing device");
                throw;
            }
            finally
            {
                logScope?.Dispose();
                _InitializingSemaphoreSlim.Release();
            }
        }

        public Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync() => Task.Run<ConnectedDeviceDefinitionBase>(() => { return AndroidUsbDeviceFactory.GetAndroidDeviceDefinition(_UsbDevice); });
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