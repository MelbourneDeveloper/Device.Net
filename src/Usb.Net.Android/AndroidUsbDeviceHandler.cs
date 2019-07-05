using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using Java.Nio;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using usbDevice = Android.Hardware.Usb.UsbDevice;

namespace Usb.Net.Android
{
    public class AndroidUsbDeviceHandler : UsbDeviceHandlerBase, IUsbDeviceHandler
    {
        #region Fields
        private UsbDeviceConnection _UsbDeviceConnection;
        private usbDevice _UsbDevice;
        private UsbEndpoint _WriteEndpoint;
        private UsbEndpoint _ReadEndpoint;
        private SemaphoreSlim _InitializingSemaphoreSlim = new SemaphoreSlim(1, 1);
        private bool _IsClosing;
        private bool disposed;
        #endregion

        #region Public Override Properties
        public bool IsInitialized => _UsbDeviceConnection != null;
        #endregion

        #region Public Properties
        public UsbManager UsbManager { get; }
        public Context AndroidContext { get; private set; }
        public ushort ReadBufferSize => (ushort)_ReadEndpoint.MaxPacketSize;
        public ushort WriteBufferSize => (ushort)_WriteEndpoint.MaxPacketSize;
        public int DeviceNumberId { get; }
        #endregion

        #region Constructor
        public AndroidUsbDeviceHandler(UsbManager usbManager, Context androidContext, int deviceNumberId, ILogger logger, ITracer tracer) : base(logger, tracer)
        {
            DeviceNumberId = deviceNumberId;
            UsbManager = usbManager;
            AndroidContext = androidContext;
            DeviceNumberId = deviceNumberId;
        }
        #endregion

        #region Public Methods 

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Close();

            _InitializingSemaphoreSlim.Dispose();

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
                _ReadEndpoint?.Dispose();
                _WriteEndpoint?.Dispose();

                _UsbDeviceConnection = null;
                _UsbDevice = null;
                _ReadEndpoint = null;
                _WriteEndpoint = null;
            }
            catch (Exception)
            {
                //TODO: Logging
            }

            _IsClosing = false;
        }

        //TODO: Make async properly
        public async Task<byte[]> ReadAsync()
        {
            try
            {
                var byteBuffer = ByteBuffer.Allocate(ReadBufferSize);
                var request = new UsbRequest();
                request.Initialize(_UsbDeviceConnection, _ReadEndpoint);
#pragma warning disable CS0618 // Type or member is obsolete
                request.Queue(byteBuffer, ReadBufferSize);
#pragma warning restore CS0618 // Type or member is obsolete
                await _UsbDeviceConnection.RequestWaitAsync();
                var buffers = new byte[ReadBufferSize];

                byteBuffer.Rewind();
                for (var i = 0; i < ReadBufferSize; i++)
                {
                    buffers[i] = (byte)byteBuffer.Get();
                }

                //Marshal.Copy(byteBuffer.GetDirectBufferAddress(), buffers, 0, ReadBufferLength);

                Tracer?.Trace(false, buffers);

                return buffers;
            }
            catch (Exception ex)
            {
                Logger?.Log(Helpers.ReadErrorMessage, nameof(AndroidUsbDeviceHandler), ex, LogLevel.Error);
                throw new IOException(Helpers.ReadErrorMessage, ex);
            }
        }

        //TODO: Perhaps we should implement Batch Begin/Complete so that the UsbRequest is not created again and again. This will be expensive
        public async Task WriteAsync(byte[] data)
        {
            try
            {
                var request = new UsbRequest();
                request.Initialize(_UsbDeviceConnection, _WriteEndpoint);
                var byteBuffer = ByteBuffer.Wrap(data);

                Tracer?.Trace(true, data);

#pragma warning disable CS0618 // Type or member is obsolete
                request.Queue(byteBuffer, data.Length);
#pragma warning restore CS0618 // Type or member is obsolete
                await _UsbDeviceConnection.RequestWaitAsync();
            }
            catch (Exception ex)
            {
                Logger?.Log(Helpers.WriteErrorMessage, nameof(AndroidUsbDeviceHandler), ex, LogLevel.Error);
                throw new IOException(Helpers.WriteErrorMessage, ex);
            }
        }

        #endregion

        #region Private  Methods
        private Task<bool?> RequestPermissionAsync()
        {
            Logger?.Log("Requesting USB permission", nameof(AndroidUsbDeviceHandler), null, LogLevel.Information);

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
            try
            {
                if (disposed) throw new Exception(DeviceBase.DeviceDisposedErrorMessage);

                await _InitializingSemaphoreSlim.WaitAsync();

                Close();

                _UsbDevice = UsbManager.DeviceList.Select(d => d.Value).FirstOrDefault(d => d.DeviceId == DeviceNumberId);

                //ConnectedDeviceDefinition = AndroidUsbDeviceFactory.GetAndroidDeviceDefinition(_UsbDevice);

                //Logger?.Log($"Found device: {ConnectedDeviceDefinition.ProductName} Id: {_UsbDevice.DeviceId}", null);

                if (_UsbDevice == null)
                {
                    throw new Exception($"The device {DeviceNumberId} is not connected to the system");
                }

                var isPermissionGranted = await RequestPermissionAsync();
                if (!isPermissionGranted.HasValue)
                {
                    throw new Exception("User did not respond to permission request");
                }

                if (!isPermissionGranted.Value)
                {
                    throw new Exception("The user did not give the permission to access the device");
                }

                //TODO: This is the default interface but other interfaces might be needed so this needs to be changed.
                var usbInterface = _UsbDevice.GetInterface(0);

                //TODO: This selection stuff needs to be moved up higher. The constructor should take these arguments
                for (var i = 0; i < usbInterface.EndpointCount; i++)
                {
                    var ep = usbInterface.GetEndpoint(i);
                    if (_ReadEndpoint == null && ep.Type == UsbAddressing.XferInterrupt && (int)ep.Address == 129)
                    {
                        _ReadEndpoint = ep;
                        continue;
                    }

                    if (_WriteEndpoint == null && ep.Type == UsbAddressing.XferInterrupt && ((int)ep.Address == 1 || (int)ep.Address == 2))
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

                if (_ReadEndpoint.MaxPacketSize != ReadBufferSize)
                {
                    throw new Exception("Wrong packet size for read endpoint");
                }

                if (_WriteEndpoint.MaxPacketSize != ReadBufferSize)
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

                Log("Hid device initialized. About to tell everyone.", null);
            }
            catch (Exception ex)
            {
                Log("Error initializing Hid Device", ex);
                throw;
            }
            finally
            {
                _InitializingSemaphoreSlim.Release();
            }
        }

        private void Log(string message, Exception ex, [CallerMemberName]string region = null)
        {
            Logger?.Log(message, region, ex, LogLevel.Error);
        }
        #endregion

        #region Finalizer
        ~AndroidUsbDeviceHandler()
        {
            Dispose();
        }
        #endregion
    }
}