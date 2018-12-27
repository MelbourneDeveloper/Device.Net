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
        private List<Interface>  _Interfaces = new List<Interface> {  };
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

            var defaultInterfaceHandle = new IntPtr();

            var isSuccess = WinUsbApiCalls.WinUsb_Initialize(_DeviceHandle, ref defaultInterfaceHandle);
            if (!isSuccess)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Couldn't initialize device. Error code: {errorCode}");
            }

            var bufferLength = (uint)Marshal.SizeOf(typeof(USB_DEVICE_DESCRIPTOR));
            isSuccess = WinUsbApiCalls.WinUsb_GetDescriptor(defaultInterfaceHandle, WinUsbApiCalls.DEFAULT_DESCRIPTOR_TYPE, 0, 0, out var deviceDesc, bufferLength, out var lengthTransfered);
            if (!isSuccess)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Couldn't get device descriptor. Error code: {errorCode}");
            }

            byte i = 0;

            //Get the first (default) interface
            var defaultInterface = GetInterface(defaultInterfaceHandle);

            _Interfaces.Add(defaultInterface);

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

                _Interfaces.Add(associatedInterface);

                i++;
            }

            IsInitialized = true;

            RaiseConnected();
        }

        private static Interface GetInterface(IntPtr interfaceHandle)
        {
            var retVal = new Interface { Handle = interfaceHandle };
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);
            if (!isSuccess)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Couldn't query interface. Error code: {errorCode}");
            }

            retVal.USB_INTERFACE_DESCRIPTOR = interfaceDescriptor;

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                retVal.Pipes.Add(new Pipe { WINUSB_PIPE_INFORMATION = pipeInfo });
            }

            return retVal;
        }

        private class Interface
        {
            public IntPtr Handle { get; set; }
            public WinUsbApiCalls.USB_INTERFACE_DESCRIPTOR USB_INTERFACE_DESCRIPTOR { get; set; }
            public List<Pipe> Pipes { get; } = new List<Pipe>();
        }

        private class Pipe
        {
            public WinUsbApiCalls.WINUSB_PIPE_INFORMATION WINUSB_PIPE_INFORMATION { get; set; }
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

            //TODO: Allow for different interfaces and pipes...
            var @interface = _Interfaces[0];
            var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(@interface.Handle, @interface.Pipes[1].WINUSB_PIPE_INFORMATION.PipeId, data,(uint) data.Length, out var bytesWritten, IntPtr.Zero);

            if (!isSuccess)
            {
                var errorCode = Marshal.GetLastWin32Error();

                throw new Exception($"Error code {errorCode}");
            }
        }
        #endregion
    }
}
