using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public sealed class WindowsUsbDevice : WindowsDeviceBase, IUsbDevice
    {
        #region Fields
        private SafeFileHandle _DeviceHandle;
        private bool disposed;
        private bool _IsClosing;
        private readonly ushort? _WriteBufferSize;
        private readonly ushort? _ReadBufferSize;
        #endregion

        #region Public Overrride Properties
        public override ushort WriteBufferSize => _WriteBufferSize ?? (IsInitialized ? (ushort)ConnectedDeviceDefinition.WriteBufferSize : (ushort)0);
        public override ushort ReadBufferSize => _ReadBufferSize ?? (IsInitialized ? (ushort)ConnectedDeviceDefinition.ReadBufferSize : (ushort)0);
        public override bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;
        public IUsbDeviceHandler UsbDeviceHandler { get; private set; }
        #endregion

        #region Constructor
        public WindowsUsbDevice(string deviceId) : this(deviceId, null, null)
        {
        }

        public WindowsUsbDevice(string deviceId, ushort? writeBufferSize, ushort? readBufferSize) : base(deviceId)
        {
            _WriteBufferSize = writeBufferSize;
            _ReadBufferSize = readBufferSize;
        }
        #endregion

        #region Private Methods
        private SafeFileHandle Initialize()
        {
            try
            {
                Close();

                int errorCode;

                if (string.IsNullOrEmpty(DeviceId))
                {
                    throw new WindowsException($"{nameof(DeviceDefinitionBase)} must be specified before {nameof(InitializeAsync)} can be called.");
                }

                _DeviceHandle = APICalls.CreateFile(DeviceId, APICalls.GenericWrite | APICalls.GenericRead, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);


                if (_DeviceHandle.IsInvalid)
                {
                    //TODO: is error code useful here?
                    errorCode = Marshal.GetLastWin32Error();
                    if (errorCode > 0) throw new Exception($"Device handle no good. Error code: {errorCode}");
                }

                var isSuccess = WinUsbApiCalls.WinUsb_Initialize(_DeviceHandle, out var defaultInterfaceHandle);
                HandleError(isSuccess, "Couldn't initialize device");

                ConnectedDeviceDefinition = GetDeviceDefinition(defaultInterfaceHandle, DeviceId);

                return defaultInterfaceHandle;
            }
            catch (Exception ex)
            {
                Logger?.Log($"{nameof(Initialize)} error. DeviceId {DeviceId}", nameof(WindowsUsbDevice), ex, LogLevel.Error);
                throw;
            }
        }
        #endregion

        #region Public Methods
        public override async Task InitializeAsync()
        {
            if (disposed) throw new Exception(DeviceDisposedErrorMessage);
            var safeFileHandle = await Task.Run(() => Initialize());
            UsbDeviceHandler = new WindowsUsbDeviceHandler(safeFileHandle);
            await UsbDeviceHandler.InitializeAsync();
        }

        public override Task<byte[]> ReadAsync()
        {
            return UsbDeviceHandler.ReadUsbInterface.ReadAsync(ReadBufferSize);
        }

        public override Task WriteAsync(byte[] data)
        {
            return UsbDeviceHandler.WriteUsbInterface.WriteAsync(data);
        }

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            try
            {
                if (UsbDeviceHandler != null)
                {
                    foreach (var usbInterface in UsbDeviceHandler.UsbInterfaces)
                    {
                        usbInterface.Dispose();
                    }

                    UsbDeviceHandler.UsbInterfaces.Clear();
                }

                _DeviceHandle?.Dispose();
                _DeviceHandle = null;
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

        #region Private Static Methods
        private static ConnectedDeviceDefinition GetDeviceDefinition(SafeFileHandle defaultInterfaceHandle, string deviceId)
        {
            var deviceDefinition = new ConnectedDeviceDefinition(deviceId) { DeviceType = DeviceType.Usb };

            var bufferLength = (uint)Marshal.SizeOf(typeof(USB_DEVICE_DESCRIPTOR));
            var isSuccess2 = WinUsbApiCalls.WinUsb_GetDescriptor(defaultInterfaceHandle, WinUsbApiCalls.DEFAULT_DESCRIPTOR_TYPE, 0, WinUsbApiCalls.EnglishLanguageID, out var _UsbDeviceDescriptor, bufferLength, out var lengthTransferred);
            HandleError(isSuccess2, "Couldn't get device descriptor");

            if (_UsbDeviceDescriptor.iProduct > 0)
            {
                deviceDefinition.ProductName = WinUsbApiCalls.GetDescriptor(defaultInterfaceHandle, _UsbDeviceDescriptor.iProduct, "Couldn't get product name");
            }

            if (_UsbDeviceDescriptor.iSerialNumber > 0)
            {
                deviceDefinition.SerialNumber = WinUsbApiCalls.GetDescriptor(defaultInterfaceHandle, _UsbDeviceDescriptor.iSerialNumber, "Couldn't get serial number");
            }

            if (_UsbDeviceDescriptor.iManufacturer > 0)
            {
                deviceDefinition.Manufacturer = WinUsbApiCalls.GetDescriptor(defaultInterfaceHandle, _UsbDeviceDescriptor.iManufacturer, "Couldn't get manufacturer");
            }

            deviceDefinition.VendorId = _UsbDeviceDescriptor.idVendor;
            deviceDefinition.ProductId = _UsbDeviceDescriptor.idProduct;
            deviceDefinition.WriteBufferSize = _UsbDeviceDescriptor.bMaxPacketSize0;
            deviceDefinition.ReadBufferSize = _UsbDeviceDescriptor.bMaxPacketSize0;

            return deviceDefinition;
        }


        #endregion

        #region Finalizer
        ~WindowsUsbDevice()
        {
            Dispose();
        }
        #endregion
    }
}
