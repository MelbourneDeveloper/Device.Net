using Device.Net;
using Device.Net.Exceptions;
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
        #endregion

        #region Public Properties
        public bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;
        public string DeviceId { get; }

        //TODO: Null checking here. These will error if the device doesn't have a value or it is not initialized
        public ushort WriteBufferSize => _WriteBufferSize.Value;
        public ushort ReadBufferSize => _ReadBufferSize.Value;
        #endregion

        #region Constructor
        public WindowsUsbDeviceHandler(string deviceId, ILogger logger, ITracer tracer, ushort? writeBufferLength, ushort? readBufferLength) : base(logger, tracer, writeBufferLength, readBufferLength)
        {
            DeviceId = deviceId;
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
                    throw new ValidationException($"{nameof(DeviceDefinitionBase)} must be specified before {nameof(InitializeAsync)} can be called.");
                }

                _DeviceHandle = APICalls.CreateFile(DeviceId, APICalls.GenericWrite | APICalls.GenericRead, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);


                if (_DeviceHandle.IsInvalid)
                {
                    //TODO: is error code useful here?
                    errorCode = Marshal.GetLastWin32Error();
                    if (errorCode > 0) throw new ApiException($"Device handle no good. Error code: {errorCode}");
                }

                var isSuccess = WinUsbApiCalls.WinUsb_Initialize(_DeviceHandle, out var defaultInterfaceHandle);
                WindowsDeviceBase.HandleError(isSuccess, Messages.ErrorMessageCouldntIntializeDevice);

                var connectedDeviceDefinition = WindowsUsbDeviceFactory.GetDeviceDefinition(defaultInterfaceHandle, DeviceId);

                if (!_WriteBufferSize.HasValue) _WriteBufferSize = (ushort)connectedDeviceDefinition.WriteBufferSize.Value;

                if (!_ReadBufferSize.HasValue) _ReadBufferSize = (ushort)connectedDeviceDefinition.ReadBufferSize.Value;

                //Get the first (default) interface
                var defaultInterface = GetInterface(defaultInterfaceHandle, _ReadBufferSize.Value, _WriteBufferSize.Value);

                UsbInterfaces.Add(defaultInterface);
                ReadUsbInterface = defaultInterface;
                WriteUsbInterface = defaultInterface;
                InterruptUsbInterface = defaultInterface;

                byte i = 0;
                while (true)
                {
                    isSuccess = WinUsbApiCalls.WinUsb_GetAssociatedInterface(defaultInterfaceHandle, i, out var interfacePointer);
                    if (!isSuccess)
                    {
                        errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == APICalls.ERROR_NO_MORE_ITEMS) break;

                        throw new ApiException($"Could not enumerate interfaces for device. Error code: { errorCode}");
                    }

                    var associatedInterface = GetInterface(interfacePointer, _ReadBufferSize.Value, _WriteBufferSize.Value);

                    //TODO: this is bad design. The handler should be taking care of this
                    UsbInterfaces.Add(associatedInterface);

                    i++;
                }
            }
            catch (Exception ex)
            {
                Logger?.Log($"{nameof(Initialize)} error. DeviceId {DeviceId}", nameof(UsbDevice), ex, LogLevel.Error);
                throw;
            }
        }

        private WindowsUsbInterface GetInterface(SafeFileHandle interfaceHandle, ushort readBufferLength, ushort writeBufferLength)
        {
            //TODO: We need to get the read/write size from a different API call...

            //TODO: Where is the logger/tracer?
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);
            var retVal = new WindowsUsbInterface(interfaceHandle, Logger, Tracer);
            WindowsDeviceBase.HandleError(isSuccess, "Couldn't query interface");

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't query endpoint");
                retVal.UsbInterfaceEndpoints.Add(new WindowsUsbInterfaceEndpoint(pipeInfo.PipeId, readBufferLength, writeBufferLength, pipeInfo.PipeType));
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

        public override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Close();

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task InitializeAsync()
        {
            await Task.Run(Initialize);
        }

        public Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync()
        {
            if (_DeviceHandle == null) throw new NotInitializedException();

            //TODO: Is this right?
            return Task.Run<ConnectedDeviceDefinitionBase>(() => { return WindowsDeviceFactoryBase.GetDeviceDefinitionFromWindowsDeviceId(DeviceId, DeviceType.Usb); });
        }
        #endregion
    }
}
