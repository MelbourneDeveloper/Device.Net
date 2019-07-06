using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using System;
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

        //TODO: Make async properly
        public Task<byte[]> ReadAsync()
        {
            return ReadUsbInterface.ReadAsync(ReadBufferSize);
        }

        //TODO: Perhaps we should implement Batch Begin/Complete so that the UsbRequest is not created again and again. This will be expensive
        public Task WriteAsync(byte[] data)
        {
            return WriteUsbInterface.WriteAsync(data);
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

                var androidUsbInterface = new AndroidUsbInterface(usbInterface, _UsbDeviceConnection, Logger, Tracer);
                UsbInterfaces.Add(androidUsbInterface);

                for (var i = 0; i < usbInterface.EndpointCount; i++)
                {
                    var ep = usbInterface.GetEndpoint(i);
                    if (ep != null)
                    {
                        var isRead = ep.Type == UsbAddressing.XferInterrupt && (int)ep.Address == 129;
                        var isWrite = ep.Type == UsbAddressing.XferInterrupt && ((int)ep.Address == 1 || (int)ep.Address == 2);
                        var androidUsbEndpoint = new AndroidUsbEndpoint(ep, isRead, isWrite, (byte)ep.Address);
                        androidUsbInterface.UsbInterfaceEndpoints.Add(androidUsbEndpoint);

                        if (androidUsbInterface.ReadEndpoint == null && isRead)
                        {
                            androidUsbInterface.ReadEndpoint = androidUsbEndpoint;
                            ReadUsbInterface = androidUsbInterface;
                        }

                        if (androidUsbInterface.WriteEndpoint == null && isWrite)
                        {
                            androidUsbInterface.WriteEndpoint = androidUsbEndpoint;
                            WriteUsbInterface = androidUsbInterface;
                        }
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