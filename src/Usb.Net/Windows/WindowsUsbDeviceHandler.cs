using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceHandler : UsbDeviceHandlerBase, IUsbDeviceHandler
    {
        #region Fields
        private bool disposed;
        private SafeFileHandle _DeviceHandle;
        private readonly ILogger _Logger;
        #endregion

        #region Public Properties
        public bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;
        public string DeviceId { get; }
        public ushort WriteBufferSize { get; private set; }
        public ushort ReadBufferSize { get; private set; }
        #endregion

        #region Constructor
        public WindowsUsbDeviceHandler(string deviceId, ILogger logger)
        {
            DeviceId = deviceId;
            _Logger = logger;
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
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't initialize device");

                var connectedDeviceDefinition = WindowsUsbDeviceFactory.GetDeviceDefinition(defaultInterfaceHandle, DeviceId);

                WriteBufferSize = (ushort)connectedDeviceDefinition.WriteBufferSize.Value;
                ReadBufferSize = (ushort)connectedDeviceDefinition.ReadBufferSize.Value;

                //Get the first (default) interface
                //TODO: It seems like there isn't a way to get other interfaces here... 😞
                var defaultInterface = GetInterface(defaultInterfaceHandle);

                UsbInterfaces.Add(defaultInterface);
                ReadUsbInterface = defaultInterface;
                WriteUsbInterface = defaultInterface;

                byte i = 0;
                while (true)
                {
                    isSuccess = WinUsbApiCalls.WinUsb_GetAssociatedInterface(defaultInterfaceHandle, i, out var interfacePointer);
                    if (!isSuccess)
                    {
                        errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == APICalls.ERROR_NO_MORE_ITEMS) break;

                        throw new Exception($"Could not enumerate interfaces for device. Error code: { errorCode}");
                    }

                    var associatedInterface = GetInterface(interfacePointer);

                    //TODO: this is bad design. The handler should be taking care of this
                    UsbInterfaces.Add(associatedInterface);

                    i++;
                }
            }
            catch (Exception ex)
            {
                _Logger?.Log($"{nameof(Initialize)} error. DeviceId {DeviceId}", nameof(UsbDevice), ex, LogLevel.Error);
                throw;
            }
        }

        private static WindowsUsbInterface GetInterface(SafeFileHandle interfaceHandle)
        {
            //TODO: Where is the logger/tracer?
            var retVal = new WindowsUsbInterface(null, null) { Handle = interfaceHandle };
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);
            WindowsDeviceBase.HandleError(isSuccess, "Couldn't query interface");

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't query endpoint");
                retVal.UsbInterfaceEndpoints.Add(new WindowsUsbInterfaceEndpoint(pipeInfo.PipeId));
            }

            return retVal;
        }
        #endregion

        #region Public Methods
        public void Close()
        {
            foreach (var usbInterface in UsbInterfaces)
            {
                usbInterface.Dispose();
            }

            UsbInterfaces.Clear();

            _DeviceHandle?.Dispose();
            _DeviceHandle = null;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Close();

            GC.SuppressFinalize(this);
        }

        public async Task InitializeAsync()
        {
            await Task.Run(Initialize);
        }
        #endregion
    }
}
