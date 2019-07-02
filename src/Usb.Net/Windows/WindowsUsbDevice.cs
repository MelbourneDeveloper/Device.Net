using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public sealed class WindowsUsbDevice : WindowsDeviceBase, IUsbDevice
    {
        #region Fields
        private SafeFileHandle _DeviceHandle;
        private readonly IList<IUsbInterface> _UsbInterfaces = new List<IUsbInterface>();
        private IUsbInterface _ReadUsbInterface;
        private IUsbInterface _WriteUsbInterface;
        private bool disposed;
        private bool _IsClosing;
        private readonly ushort? _WriteBufferSize;
        private readonly ushort? _ReadBufferSize;
        #endregion

        #region Public Overrride Properties
        public override ushort WriteBufferSize => _WriteBufferSize ?? (IsInitialized ? (ushort)ConnectedDeviceDefinition.WriteBufferSize : (ushort)0);
        public override ushort ReadBufferSize => _ReadBufferSize ?? (IsInitialized ? (ushort)ConnectedDeviceDefinition.ReadBufferSize : (ushort)0);
        public override bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;

        public IUsbInterface ReadUsbInterface
        {
            get => _ReadUsbInterface;
            set
            {
                if (!UsbInterfaces.Contains(value)) throw new Exception("The interface is not contained the list of valid interfaces.");
                _ReadUsbInterface = value;
            }
        }

        public IUsbInterface WriteUsbInterface
        {
            get => _WriteUsbInterface;
            set
            {
                if (!UsbInterfaces.Contains(value)) throw new Exception("The interface is not contained the list of valid interfaces.");
                _WriteUsbInterface = value;
            }
        }
        #endregion

        #region Public Properties
        public IList<IUsbInterface> UsbInterfaces { get; } = new List<IUsbInterface>();
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
        private void Initialize()
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

                byte i = 0;

                //Get the first (default) interface
                var defaultInterface = GetInterface(defaultInterfaceHandle);

                UsbInterfaces.Add(defaultInterface);

                ReadUsbInterface = defaultInterface;
                WriteUsbInterface = defaultInterface;

                while (true)
                {
                    isSuccess = WinUsbApiCalls.WinUsb_GetAssociatedInterface(defaultInterfaceHandle, i, out var interfacePointer);
                    if (!isSuccess)
                    {
                        errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == APICalls.ERROR_NO_MORE_ITEMS) break;

                        throw new Exception($"Could not enumerate interfaces for device {DeviceId}. Error code: { errorCode}");
                    }

                    var associatedInterface = GetInterface(interfacePointer);

                    _UsbInterfaces.Add(associatedInterface);

                    i++;
                }
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
            await Task.Run(() => Initialize());
        }

        public override Task<byte[]> ReadAsync()
        {
            return ReadUsbInterface.ReadAsync(ReadBufferSize);
        }

        public override Task WriteAsync(byte[] data)
        {
            return WriteUsbInterface.WriteAsync(data);
        }

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            try
            {
                foreach (var usbInterface in UsbInterfaces)
                {
                    usbInterface.Dispose();
                }

                UsbInterfaces.Clear();

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

        private static WindowsUsbInterface GetInterface(SafeFileHandle interfaceHandle)
        {
            //TODO: Where is the logger/tracer?
            var retVal = new WindowsUsbInterface(null, null) { Handle = interfaceHandle };
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);
            HandleError(isSuccess, "Couldn't query interface");

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                HandleError(isSuccess, "Couldn't query endpoint");
                retVal.UsbInterfaceEndpoints.Add(new WindowsUsbInterfaceEndpoint(pipeInfo.PipeId));
            }

            return retVal;
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
