using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterfaceManager : UsbInterfaceManager, IUsbInterfaceManager
    {
        #region Fields
        private bool disposed;
        private SafeFileHandle? _DeviceHandle;
        private readonly ushort? _ReadBufferSize;
        private readonly ushort? _WriteBufferSize;
        #endregion

        #region Public Properties
        public bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;
        public string DeviceId { get; }

        //TODO: Null checking here. These will error if the device doesn't have a value or it is not initialized
        public ushort WriteBufferSize => _WriteBufferSize ??
            WriteUsbInterface.NullCheck(Messages.ErrorMessageNotInitialized).ReadBufferSize;

        public ushort ReadBufferSize => _ReadBufferSize ??
            ReadUsbInterface.NullCheck(Messages.ErrorMessageNotInitialized).ReadBufferSize;

        #endregion

        #region Constructor
        public WindowsUsbInterfaceManager(
            string deviceId,
            ILoggerFactory? loggerFactory = null,
            ushort? readBufferLength = null,
            ushort? writeBufferLength = null) : base(loggerFactory)
        {
            _ReadBufferSize = readBufferLength;
            _WriteBufferSize = writeBufferLength;
            DeviceId = deviceId;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            using var logScope = Logger.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(Initialize));

            try
            {

                Close();

                int errorCode;

                if (string.IsNullOrEmpty(DeviceId))
                {
                    throw new ValidationException(
                        $"{nameof(ConnectedDeviceDefinition)} must be specified before {nameof(InitializeAsync)} can be called.");
                }

                _DeviceHandle = APICalls.CreateFile(DeviceId,
                    FileAccessRights.GenericWrite | FileAccessRights.GenericRead,
                    APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting,
                    APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);

                if (_DeviceHandle.IsInvalid)
                {
                    //TODO: is error code useful here?
                    errorCode = Marshal.GetLastWin32Error();
                    if (errorCode > 0) throw new ApiException($"Device handle no good. Error code: {errorCode}");
                }

                Logger.LogInformation(Messages.SuccessMessageGotWriteAndReadHandle);

                var isSuccess = WinUsbApiCalls.WinUsb_Initialize(_DeviceHandle, out var interfaceHandle);
                _ = WindowsHelpers.HandleError(isSuccess, Messages.ErrorMessageCouldntIntializeDevice, Logger);

#pragma warning disable CA2000 //We need to hold on to this handle
                var defaultInterfaceHandle = new SafeFileHandle(interfaceHandle, false);
#pragma warning restore CA2000
                var connectedDeviceDefinition = GetDeviceDefinition(defaultInterfaceHandle, DeviceId, Logger);

                if (!_WriteBufferSize.HasValue && !connectedDeviceDefinition.WriteBufferSize.HasValue)
                    throw new ValidationException("Write buffer size not specified");

                if (!_ReadBufferSize.HasValue && !connectedDeviceDefinition.ReadBufferSize.HasValue)
                    throw new ValidationException("Read buffer size not specified");

                //Get the first (default) interface
#pragma warning disable CA2000 //Ths should be disposed later
                var defaultInterface = GetInterface(defaultInterfaceHandle);

                UsbInterfaces.Add(defaultInterface);

                byte i = 0;
                while (true)
                {
                    isSuccess = WinUsbApiCalls.WinUsb_GetAssociatedInterface(defaultInterfaceHandle, i,
                        out var interfacePointer);
                    if (!isSuccess)
                    {
                        errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == APICalls.ERROR_NO_MORE_ITEMS) break;

                        throw new ApiException(
                            $"Could not enumerate interfaces for device. Error code: {errorCode}");
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
                Logger.LogError(ex, Messages.ErrorMessageCouldntIntializeDevice);
                throw;
            }
        }

        private WindowsUsbInterface GetInterface(SafeFileHandle interfaceHandle)
        {
            //TODO: We need to get the read/write size from a different API call...

            //TODO: Where is the logger/tracer?
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);

            var retVal = new WindowsUsbInterface(
                interfaceHandle,
                interfaceDescriptor.bInterfaceNumber,
                Logger,
                ReadBufferSize,
                WriteBufferSize);
            _ = WindowsHelpers.HandleError(isSuccess, "Couldn't query interface", Logger);

            Logger.LogInformation(
                "Found Interface Number: {interfaceNumber} Endpoint count: {endpointCount} Class: {class} Subclass: {subClass}",
                interfaceDescriptor.bInterfaceNumber,
                interfaceDescriptor.bNumEndpoints,
                interfaceDescriptor.bInterfaceClass,
                interfaceDescriptor.bInterfaceSubClass);

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                _ = WindowsHelpers.HandleError(isSuccess, "Couldn't query endpoint", Logger);

                Logger.LogInformation("Found PipeId: {pipeId} PipeType: {pipeType} MaxPacketSize: {maxPacketSize}", pipeInfo.PipeId, pipeInfo.PipeType, pipeInfo.MaximumPacketSize);

                //TODO: We are dropping the max packet size here...

                retVal.UsbInterfaceEndpoints.Add(new WindowsUsbInterfaceEndpoint(pipeInfo.PipeId, pipeInfo.PipeType, pipeInfo.MaximumPacketSize));
            }

            return retVal;
        }
        #endregion

        #region Public Methods

        private static ConnectedDeviceDefinition GetDeviceDefinition(SafeFileHandle defaultInterfaceHandle, string deviceId, ILogger logger)
        {
            if (defaultInterfaceHandle.IsInvalid) throw new InvalidOperationException("Interface handle invalid");

            var bufferLength = (uint)Marshal.SizeOf(typeof(USB_DEVICE_DESCRIPTOR));
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var isSuccess2 = WinUsbApiCalls.WinUsb_GetDescriptor(defaultInterfaceHandle, WinUsbApiCalls.DEFAULT_DESCRIPTOR_TYPE, 0, WinUsbApiCalls.EnglishLanguageID, out var _UsbDeviceDescriptor, bufferLength, out var lengthTransferred);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            _ = WindowsHelpers.HandleError(isSuccess2, "Couldn't get device descriptor", logger);

            string? productName = null;
            string? serialNumber = null;
            string? manufacturer = null;

            if (_UsbDeviceDescriptor.iProduct > 0)
            {
                productName = WinUsbApiCalls.GetDescriptor(
                    defaultInterfaceHandle,
                    _UsbDeviceDescriptor.iProduct,
                    "Couldn't get product name",
                    logger);
            }

            if (_UsbDeviceDescriptor.iSerialNumber > 0)
            {
                serialNumber = WinUsbApiCalls.GetDescriptor(
                    defaultInterfaceHandle,
                    _UsbDeviceDescriptor.iSerialNumber,
                    "Couldn't get serial number",
                    logger);
            }

            if (_UsbDeviceDescriptor.iManufacturer > 0)
            {
                manufacturer = WinUsbApiCalls.GetDescriptor(
                    defaultInterfaceHandle,
                    _UsbDeviceDescriptor.iManufacturer,
                    "Couldn't get manufacturer",
                    logger);
            }

            return new ConnectedDeviceDefinition(
                deviceId,
                DeviceType.Usb,
                productName: productName,
                serialNumber: serialNumber,
                manufacturer: manufacturer,
                vendorId: _UsbDeviceDescriptor.idVendor,
                productId: _UsbDeviceDescriptor.idProduct,
                writeBufferSize: _UsbDeviceDescriptor.bMaxPacketSize0,
                readBufferSize: _UsbDeviceDescriptor.bMaxPacketSize0
                );
        }

        public override void Close()
        {
            _DeviceHandle?.Dispose();
            _DeviceHandle = null;

            base.Close();
        }

        public override void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, DeviceId);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, DeviceId);

            Close();

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default) => await Task.Run(Initialize, cancellationToken).ConfigureAwait(false);

        public Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync(CancellationToken cancellationToken = default)
        {
            if (_DeviceHandle == null) throw new NotInitializedException();

            //TODO: Is this right?
            return Task.Run(() => DeviceBase.GetDeviceDefinitionFromWindowsDeviceId(DeviceId, DeviceType.Usb, Logger), cancellationToken);
        }
        #endregion
    }
}
