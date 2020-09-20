using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterfaceManager : UsbInterfaceManager, IUsbInterfaceManager
    {
        #region Fields
        private bool disposed;
        private SafeFileHandle _DeviceHandle;
        protected ushort? ReadBufferSizeProtected { get; set; }
        protected ushort? WriteBufferSizeProtected { get; set; }
        #endregion

        #region Public Properties
        public bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;
        public string DeviceId { get; }

        //TODO: Null checking here. These will error if the device doesn't have a value or it is not initialized
        public ushort WriteBufferSize => WriteBufferSizeProtected ?? WriteUsbInterface.ReadBufferSize;
        public ushort ReadBufferSize => ReadBufferSizeProtected ?? ReadUsbInterface.ReadBufferSize;
        #endregion

        #region Constructor
        public WindowsUsbInterfaceManager(string deviceId, ILoggerFactory loggerFactory, ushort? readBufferLength, ushort? writeBufferLength) : base(loggerFactory)
        {
            ReadBufferSizeProtected = readBufferLength;
            WriteBufferSizeProtected = writeBufferLength;
            DeviceId = deviceId;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(Initialize));

                Close();

                int errorCode;

                if (string.IsNullOrEmpty(DeviceId))
                {
                    throw new ValidationException($"{nameof(DeviceDefinitionBase)} must be specified before {nameof(InitializeAsync)} can be called.");
                }

                _DeviceHandle = APICalls.CreateFile(DeviceId, FileAccessRights.GenericWrite | FileAccessRights.GenericRead, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);

                if (_DeviceHandle.IsInvalid)
                {
                    //TODO: is error code useful here?
                    errorCode = Marshal.GetLastWin32Error();
                    if (errorCode > 0) throw new ApiException($"Device handle no good. Error code: {errorCode}");
                }

                Logger?.LogInformation(Messages.SuccessMessageGotWriteAndReadHandle);

#pragma warning disable CA2000 //We need to hold on to this handle
                var isSuccess = WinUsbApiCalls.WinUsb_Initialize(_DeviceHandle, out var defaultInterfaceHandle);
#pragma warning restore CA2000 
                WindowsDeviceBase.HandleError(isSuccess, Messages.ErrorMessageCouldntIntializeDevice);

                var connectedDeviceDefinition = GetDeviceDefinition(defaultInterfaceHandle, DeviceId, Logger);

                if (!WriteBufferSizeProtected.HasValue)
                {
                    if (!connectedDeviceDefinition.WriteBufferSize.HasValue) throw new ValidationException("Write buffer size not specified");
                    WriteBufferSizeProtected = (ushort)connectedDeviceDefinition.WriteBufferSize.Value;
                }

                if (!ReadBufferSizeProtected.HasValue)
                {
                    if (!connectedDeviceDefinition.ReadBufferSize.HasValue) throw new ValidationException("Read buffer size not specified");
                    ReadBufferSizeProtected = (ushort)connectedDeviceDefinition.ReadBufferSize.Value;
                }

                //Get the first (default) interface
#pragma warning disable CA2000 //Ths should be disposed later
                var defaultInterface = GetInterface(defaultInterfaceHandle);

                UsbInterfaces.Add(defaultInterface);

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

                    var associatedInterface = GetInterface(interfacePointer);

                    //TODO: this is bad design. The handler should be taking care of this
                    UsbInterfaces.Add(associatedInterface);

                    i++;
                }

                RegisterDefaultInterfaces();
#pragma warning restore CA2000
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageCouldntIntializeDevice);
                throw;
            }
            finally
            {
                logScope?.Dispose();
            }
        }

        private WindowsUsbInterface GetInterface(SafeFileHandle interfaceHandle)
        {
            //TODO: We need to get the read/write size from a different API call...

            //TODO: Where is the logger/tracer?
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);

            var retVal = new WindowsUsbInterface(interfaceHandle, Logger, interfaceDescriptor.bInterfaceNumber, ReadBufferSizeProtected, WriteBufferSizeProtected);
            WindowsDeviceBase.HandleError(isSuccess, "Couldn't query interface");

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't query endpoint");
                retVal.UsbInterfaceEndpoints.Add(new WindowsUsbInterfaceEndpoint(pipeInfo.PipeId, pipeInfo.PipeType));
            }

            return retVal;
        }
        #endregion

        #region Public Methods
        public static ConnectedDeviceDefinition GetDeviceDefinition(SafeFileHandle defaultInterfaceHandle, string deviceId, ILogger logger)
        {
            var deviceDefinition = new ConnectedDeviceDefinition(deviceId) { DeviceType = DeviceType.Usb };

            var bufferLength = (uint)Marshal.SizeOf(typeof(USB_DEVICE_DESCRIPTOR));
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var isSuccess2 = WinUsbApiCalls.WinUsb_GetDescriptor(defaultInterfaceHandle, WinUsbApiCalls.DEFAULT_DESCRIPTOR_TYPE, 0, WinUsbApiCalls.EnglishLanguageID, out var _UsbDeviceDescriptor, bufferLength, out var lengthTransferred);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            WindowsDeviceBase.HandleError(isSuccess2, "Couldn't get device descriptor");

            if (_UsbDeviceDescriptor.iProduct > 0)
            {
                deviceDefinition.ProductName = WinUsbApiCalls.GetDescriptor(
                    defaultInterfaceHandle,
                    _UsbDeviceDescriptor.iProduct,
                    "Couldn't get product name",
                    logger);
            }

            if (_UsbDeviceDescriptor.iSerialNumber > 0)
            {
                deviceDefinition.SerialNumber = WinUsbApiCalls.GetDescriptor(defaultInterfaceHandle,
                                                                             _UsbDeviceDescriptor.iSerialNumber,
                                                                             "Couldn't get serial number",
                                                                             logger);
            }

            if (_UsbDeviceDescriptor.iManufacturer > 0)
            {
                deviceDefinition.Manufacturer = WinUsbApiCalls.GetDescriptor(
                    defaultInterfaceHandle,
                    _UsbDeviceDescriptor.iManufacturer,
                    "Couldn't get manufacturer",
                    logger);
            }

            deviceDefinition.VendorId = _UsbDeviceDescriptor.idVendor;
            deviceDefinition.ProductId = _UsbDeviceDescriptor.idProduct;
            deviceDefinition.WriteBufferSize = _UsbDeviceDescriptor.bMaxPacketSize0;
            deviceDefinition.ReadBufferSize = _UsbDeviceDescriptor.bMaxPacketSize0;

            return deviceDefinition;
        }

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

        public async Task InitializeAsync() => await Task.Run(Initialize);

        public Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync()
        {
            if (_DeviceHandle == null) throw new NotInitializedException();

            //TODO: Is this right?
            return Task.Run<ConnectedDeviceDefinitionBase>(() => { return DeviceBase.GetDeviceDefinitionFromWindowsDeviceId(DeviceId, DeviceType.Usb, Logger); });
        }
        #endregion
    }
}
