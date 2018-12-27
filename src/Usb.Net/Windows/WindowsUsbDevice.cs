using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbDevice : WindowsDeviceBase
    {
        #region Fields
        private SafeFileHandle _DeviceHandle;
        #endregion

        #region Public Overrride Properties
        public override ushort WriteBufferSize { get; }
        public override ushort ReadBufferSize { get; }
        #endregion

        #region Constructor
        public WindowsUsbDevice(string deviceId, ushort writeBufferSzie, ushort readBufferSize) : base(deviceId)
        {
            WriteBufferSize = writeBufferSzie;
            ReadBufferSize = readBufferSize;
        }
        #endregion

        #region Public Methods
        public override async Task InitializeAsync()
        {
            Dispose();

            int errorCode;

            if (string.IsNullOrEmpty(DeviceId))
            {
                throw new WindowsException($"{nameof(DeviceDefinition)} must be specified before {nameof(InitializeAsync)} can be called.");
            }

            _DeviceHandle = APICalls.CreateFile(DeviceId, (APICalls.GenericWrite | APICalls.GenericRead), APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);

            if (_DeviceHandle.IsInvalid)
            {
                //TODO: is error code useful here?
                errorCode = Marshal.GetLastWin32Error();
                if (errorCode > 0) throw new Exception($"Device handle no good. Error code: {errorCode}");
            }

            var interfaceHandle = new IntPtr();

            var isSuccess = WinUsbApiCalls.WinUsb_Initialize(_DeviceHandle, ref interfaceHandle);
            if (!isSuccess)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Couldn't initialize device. Error code: {errorCode}");
            }

            byte i = 0;

            var interfacePointers = new List<IntPtr>();

            while (true)
            {
                var interfacePointer = IntPtr.Zero;
                isSuccess = WinUsbApiCalls.WinUsb_GetAssociatedInterface(interfaceHandle, i, out interfacePointer);
                if (!isSuccess)
                {
                    errorCode = Marshal.GetLastWin32Error();
                    if (errorCode == APICalls.ERROR_NO_MORE_ITEMS) break;

                    throw new Exception($"Could not enumerate interfaces for device {DeviceId}. Error code: { errorCode}");
                }

                interfacePointers.Add(interfacePointer);
                i++;
            }

            var bufferLength = (uint)Marshal.SizeOf(typeof(USB_DEVICE_DESCRIPTOR));
            isSuccess = WinUsbApiCalls.WinUsb_GetDescriptor(interfaceHandle, WinUsbApiCalls.DEFAULT_DESCRIPTOR_TYPE , 0, 0, out var deviceDesc, bufferLength, out var lengthTransfered);
            if (!isSuccess)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Couldn't get device descriptor. Error code: {errorCode}");
            }

            IsInitialized = true;

            RaiseConnected();
        }

        public override async Task<byte[]> ReadAsync()
        {
            var bytes = new byte[ReadBufferSize];

            var isSuccess = APICalls.ReadFile(_DeviceHandle, bytes, ReadBufferSize, out var asdds, 0);

            var errorCode = Marshal.GetLastWin32Error();

            if (!isSuccess)
            {
                throw new Exception($"Error code {errorCode}");
            }

            Tracer?.Trace(false, bytes);

            return bytes;
        }

        public override async Task WriteAsync(byte[] data)
        {
            if (data.Length > WriteBufferSize)
            {
                throw new Exception($"Data is longer than {WriteBufferSize} bytes which is the device's OutputReportByteLength.");
            }

            var isSuccess = APICalls.WriteFile(_DeviceHandle, data, (uint)data.Length, out var bytesWritten, 0);

            var errorCode = Marshal.GetLastWin32Error();

            if (!isSuccess)
            {
                throw new Exception($"Error code {errorCode}");
            }
        }
        #endregion
    }
}
