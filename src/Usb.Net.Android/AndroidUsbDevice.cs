using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using Java.Nio;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Android
{
    public class AndroidUsbDevice : DeviceBase, IDevice
    {
        #region Fields
        private UsbDeviceConnection _UsbDeviceConnection;
        private UsbDevice _UsbDevice;
        private UsbEndpoint _WriteEndpoint;
        private UsbEndpoint _ReadEndpoint;
        private bool _IsInitializing;
        #endregion

        #region Public Constants
        public const string LogSection = "AndroidHidDevice";
        #endregion

        #region Public Properties
        public UsbManager UsbManager { get; }
        public Context AndroidContext { get; private set; }
        public int TimeoutMilliseconds { get; }
        public int ReadBufferLength { get; }
        public int VendorId { get; }
        public int ProductId { get; }
        public bool IsInitialized { get; private set; }
        #endregion

        #region Events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        #endregion

        #region Constructor
        public AndroidUsbDevice(UsbManager usbManager, Context androidContext, int timeoutMilliseconds, int readBufferLength, int vendorId, int productId)
        {
            UsbManager = usbManager;
            AndroidContext = androidContext;
            TimeoutMilliseconds = timeoutMilliseconds;
            ReadBufferLength = readBufferLength;
            VendorId = vendorId;
            ProductId = productId;

            //TODO: Remove this. The device needs to be initialized properly
            //Check to see if the device is connected asynchronously.
            CheckForDeviceAsync();
        }
        #endregion

        #region Public Methods 

        public async Task<bool> GetIsConnectedAsync()
        {
            try
            {
                if (_UsbDeviceConnection == null)
                {
                    Logger.Log($"{nameof(_UsbDeviceConnection)} is null", null, LogSection);

                    await CheckForDeviceAsync();

                    return _UsbDeviceConnection != null;
                }

                Logger.Log("Android Hid device is connected", null, LogSection);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Error getting IsConnected on Android device", ex, LogSection);
                throw;
            }
        }

        public async Task UsbDeviceAttached()
        {
            Logger.Log("Device attached", null, LogSection);
            await CheckForDeviceAsync();
        }

        public async Task UsbDeviceDetached()
        {
            Logger.Log("Device detached", null, LogSection);
            await CheckForDeviceAsync();
        }

        public void Dispose()
        {
            _UsbDeviceConnection?.Dispose();
            _UsbDevice?.Dispose();
            _ReadEndpoint?.Dispose();
            _WriteEndpoint?.Dispose();
        }

        //TODO: Make async properly
        public override async Task<byte[]> ReadAsync()
        {
            try
            {
                var byteBuffer = ByteBuffer.Allocate(ReadBufferLength);
                var request = new UsbRequest();
                request.Initialize(_UsbDeviceConnection, _ReadEndpoint);
                request.Queue(byteBuffer, ReadBufferLength);
                await _UsbDeviceConnection.RequestWaitAsync();
                var buffers = new byte[ReadBufferLength];

                byteBuffer.Rewind();
                for (var i = 0; i < ReadBufferLength; i++)
                {
                    buffers[i] = (byte)byteBuffer.Get();
                }

                //Marshal.Copy(byteBuffer.GetDirectBufferAddress(), buffers, 0, ReadBufferLength);

                Tracer?.Trace(false, buffers);

                return buffers;
            }
            catch (Exception ex)
            {
                Logger.Log(Helpers.ReadErrorMessage, ex, LogSection);
                throw new IOException(Helpers.ReadErrorMessage, ex);
            }
        }

        //TODO: Perhaps we should implement Batch Begin/Complete so that the UsbRequest is not created again and again. This will be expensive
        public  override async Task WriteAsync(byte[] data)
        {
            try
            {
                var request = new UsbRequest();
                request.Initialize(_UsbDeviceConnection, _WriteEndpoint);
                var byteBuffer = ByteBuffer.Wrap(data);

                Tracer?.Trace(true, data);

                request.Queue(byteBuffer, data.Length);
                await _UsbDeviceConnection.RequestWaitAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(Helpers.WriteErrorMessage, ex, LogSection);
                throw new IOException(Helpers.WriteErrorMessage, ex);
            }
        }

        #endregion

        #region Private  Methods

        private async Task CheckForDeviceAsync()
        {
            var devices = UsbManager.DeviceList.Select(kvp => kvp.Value).ToList();

            Logger.Log($"Connected devices: {string.Join(",", devices.Select(d => d.VendorId))}.", null, LogSection);

            _UsbDevice?.Dispose();
            _UsbDevice = devices.FirstOrDefault(d => d.VendorId == VendorId && d.ProductId == ProductId);

            if (_UsbDevice != null)
            {
                if (_UsbDeviceConnection == null)
                {
                    Logger.Log("Initializing Android Hid device", null, LogSection);
                    await InitializeAsync();
                }
            }
            else
            {
                var wasConnected = _UsbDeviceConnection != null;
                _UsbDeviceConnection?.Dispose();
                _UsbDeviceConnection = null;
                if (wasConnected)
                {
                    Disconnected?.Invoke(this, new EventArgs());
                }
            }
        }

        private Task<bool?> RequestPermissionAsync()
        {
            Logger.Log("Requesting USB permission", null, LogSection);

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
            //TODO: Use a semaphore lock here
            if (_IsInitializing)
            {
                return;
            }

            _IsInitializing = true;

            try
            {
                //TODO:
                //Dispose();

                var isPermissionGranted = await RequestPermissionAsync();
                if (!isPermissionGranted.HasValue)
                {
                    throw new Exception("User did not respond to permission request");
                }

                if (!isPermissionGranted.Value)
                {
                    throw new Exception("The user did not give the permission to access the device");
                }

                var usbInterface = _UsbDevice.GetInterface(0);

                //TODO: This selection stuff needs to be moved up higher. The constructor should take these arguments
                for (var i = 0; i < usbInterface.EndpointCount; i++)
                {
                    var ep = usbInterface.GetEndpoint(i);
                    if (_ReadEndpoint == null && ep.Type == UsbAddressing.XferInterrupt && ep.Address == (UsbAddressing)129)
                    {
                        _ReadEndpoint = ep;
                        continue;
                    }

                    if (_WriteEndpoint == null && ep.Type == UsbAddressing.XferInterrupt && (ep.Address == (UsbAddressing)1 || ep.Address == (UsbAddressing)2))
                    {
                        _WriteEndpoint = ep;
                    }
                }

                //TODO: This is a bit of a guess. It only kicks in if the previous code fails. This needs to be reworked for different devices
                if (_ReadEndpoint == null)
                {
                    _ReadEndpoint = usbInterface.GetEndpoint(0);
                }

                if (_WriteEndpoint == null)
                {
                    _WriteEndpoint = usbInterface.GetEndpoint(1);
                }

                if (_ReadEndpoint.MaxPacketSize != ReadBufferLength)
                {
                    throw new Exception("Wrong packet size for read endpoint");
                }

                if (_WriteEndpoint.MaxPacketSize != ReadBufferLength)
                {
                    throw new Exception("Wrong packet size for write endpoint");
                }

                _UsbDeviceConnection = UsbManager.OpenDevice(_UsbDevice);

                if (_UsbDeviceConnection == null)
                {
                    throw new Exception("could not open connection");
                }

                if (!_UsbDeviceConnection.ClaimInterface(usbInterface, true))
                {
                    throw new Exception("could not claim interface");
                }

                Logger.Log("Hid device initialized. About to tell everyone.", null, LogSection);

                IsInitialized = true;

                Connected?.Invoke(this, new EventArgs());

                return;
            }
            catch (Exception ex)
            {
                Logger.Log("Error initializing Hid Device", ex, LogSection);
            }

            _IsInitializing = false;
        }
        #endregion
    }
}