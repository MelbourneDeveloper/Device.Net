using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbDevice : WindowsDeviceBase
    {
        #region Fields
        private SafeFileHandle _DeviceHandle;
        #endregion

        #region Public Methods
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

        public override async Task InitializeAsync()
        {
            Dispose();

            if (string.IsNullOrEmpty(DeviceId))
            {
                throw new WindowsException($"{nameof(DeviceDefinition)} must be specified before {nameof(InitializeAsync)} can be called.");
            }

            _DeviceHandle = APICalls.CreateFile(DeviceId, (APICalls.GenericWrite | APICalls.GenericRead), APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);

            var errorCode = Marshal.GetLastWin32Error();

            if (errorCode > 0) throw new Exception($"Write handle no good. Error code: {errorCode}");

            var interfaceHandle = new IntPtr();

            var isSuccess = WinUsbApiCalls.WinUsb_Initialize(_DeviceHandle, ref interfaceHandle);

            errorCode = Marshal.GetLastWin32Error();

            if (!isSuccess) throw new Exception($"Initialization failed. Error code: {errorCode}");

            IsInitialized = true;

            RaiseConnected();
        }

        //For posterity
        //public override async Task InitializeAsync()
        //{
        //    Dispose();

        //    if (string.IsNullOrEmpty(DeviceId))
        //    {
        //        throw new WindowsException($"{nameof(DeviceDefinition)} must be specified before {nameof(InitializeAsync)} can be called.");
        //    }

        //    _DeviceHandle = APICalls.CreateFile(DeviceId, (APICalls.GenericWrite | APICalls.GenericRead), APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);

        //    var errorCode = Marshal.GetLastWin32Error();

        //    if (errorCode > 0) throw new Exception($"Write handle no good. Error code: {errorCode}");

        //    var interfaceHandle = new IntPtr();

        //    var pDll = NativeMethods.LoadLibrary(@"C:\GitRepos\Device.Net\src\Usb.Net.WindowsSample\bin\Debug\net452\winusb.dll");

        //    var pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "WinUsb_Initialize");

        //    var initialize = (WinUsb_Initialize)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(WinUsb_Initialize));

        //    var isSuccess = initialize(_DeviceHandle, ref interfaceHandle);

        //    errorCode = Marshal.GetLastWin32Error();

        //    if (!isSuccess) throw new Exception($"Initialization failed. Error code: {errorCode}");

        //    IsInitialized = true;

        //    RaiseConnected();
        //}

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //private delegate bool WinUsb_Initialize(SafeFileHandle DeviceHandle, ref IntPtr InterfaceHandle);

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
    }
}
