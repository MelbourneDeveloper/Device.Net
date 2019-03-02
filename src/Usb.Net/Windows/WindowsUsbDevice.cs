using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public sealed class WindowsUsbDevice : WindowsDeviceBase, IDevice
    {
        #region Fields
        private SafeFileHandle _DeviceHandle;
        private readonly List<UsbInterface> _UsbInterfaces = new List<UsbInterface>();
        private UsbInterface _DefaultUsbInterface => _UsbInterfaces.FirstOrDefault();
        private bool disposed;
        private bool _IsClosing;
        #endregion

        #region Public Overrride Properties
        public override ushort WriteBufferSize => IsInitialized ? (ushort)ConnectedDeviceDefinition.WriteBufferSize : (ushort)0;
        public override ushort ReadBufferSize => IsInitialized ? (ushort)ConnectedDeviceDefinition.ReadBufferSize : (ushort)0;
        public override bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;
        #endregion

        #region Constructor
        public WindowsUsbDevice(string deviceId) : base(deviceId)
        {
        }
        #endregion

        #region Private Methods
        private void Initialize()
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

            _UsbInterfaces.Add(defaultInterface);

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
        #endregion

        #region Public Methods
        public override async Task InitializeAsync()
        {
            if (disposed) throw new Exception(DeviceDisposedErrorMessage);
            await Task.Run(() => Initialize());
        }

        public override async Task<byte[]> ReadAsync()
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[ReadBufferSize];
                //TODO: Allow for different interfaces and pipes...
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_DefaultUsbInterface.Handle, _DefaultUsbInterface.ReadPipe.WINUSB_PIPE_INFORMATION.PipeId, bytes, ReadBufferSize, out var bytesRead, IntPtr.Zero);
                HandleError(isSuccess, "Couldn't read data");
                Tracer?.Trace(false, bytes);
                return bytes;
            });
        }

        public override async Task WriteAsync(byte[] data)
        {
            await Task.Run(() =>
            {
                if (data.Length > WriteBufferSize)
                {
                    throw new Exception($"Data is longer than {WriteBufferSize} bytes which is the device's max buffer size.");
                }

                //TODO: Allow for different interfaces and pipes...
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(_DefaultUsbInterface.Handle, _DefaultUsbInterface.WritePipe.WINUSB_PIPE_INFORMATION.PipeId, data, (uint)data.Length, out var bytesWritten, IntPtr.Zero);
                HandleError(isSuccess, "Couldn't write data");
                Tracer?.Trace(true, data);
            });
        }

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            try
            {
                foreach (var usbInterface in _UsbInterfaces)
                {
                    usbInterface.Dispose();
                }

                _UsbInterfaces.Clear();

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

        private static UsbInterface GetInterface(SafeFileHandle interfaceHandle)
        {
            var retVal = new UsbInterface { Handle = interfaceHandle };
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);
            HandleError(isSuccess, "Couldn't query interface");

            retVal.USB_INTERFACE_DESCRIPTOR = interfaceDescriptor;

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                HandleError(isSuccess, "Couldn't query pipe");
                retVal.UsbInterfacePipes.Add(new UsbInterfacePipe { WINUSB_PIPE_INFORMATION = pipeInfo });
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
