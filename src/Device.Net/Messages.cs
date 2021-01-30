// ReSharper disable UnusedMember.Global

namespace Device.Net
{
    internal static class Messages
    {
        #region Code Specific Messages
        public const string ObsoleteMessagePlatformSpecificUsbDevice = "Platform specific USB Devices are being deprecated. Please construct a UsbDevice and pass the UsbInterfaceManager in to the constructor. This is to maintain the dependency injection pattern.";
        #endregion

        #region Success Messages
        public const string SuccessMessageGotWriteAndReadHandle = "Successfully opened handle on device for reading and writing";
        public const string SuccessMessageWriteAndReadCalled = "Successfully called write and read";
        public const string SuccessMessageReadFileStreamOpened = "Read file stream opened successfully";
        public const string SuccessMessageWriteFileStreamOpened = "Write file stream opened successfully";
        #endregion

        #region Warnings
        public const string WarningMessageOpeningInReadonlyMode = "Opening device {deviceId} in read only mode.";
        public const string WarningMessageReadFileStreamCantRead = "Read file stream cannot be read from";
        public const string WarningMessageWriteFileStreamCantWrite = "Write file stream cannot be written to";
        public const string WarningMessageAlreadyDisposed = "Attempted to dispose already disposed device {deviceId}";
        #endregion

        #region Device Initialization
        public const string InformationMessageInitializingDevice = "Initializing device...";
        public const string ErrorMessageNotInitialized = "The device has not been initialized.";
        public const string ErrorMessageCouldntIntializeDevice = "Couldn't initialize device.";
        public const string ErrorMessageCantOpenWrite = "Could not open connection for writing";
        public const string ErrorMessageCantOpenRead = "Could not open connection for reading";
        public const string DeviceDisposedErrorMessage = "This device has already been disposed";
        public static string GetErrorMessageCantConnect(string deviceId) => $"Could not connect to device with Device Id {deviceId}. Check that the package manifest has been configured to allow this device.";
        public const string ErrorMessageCantClose = "Error closing device. DeviceId: {deviceId} Region: {region}";
        #endregion

        #region Misc
        public const string ErrorMessageFlushNotImplemented = "Flush has only been implemented on Serial Port devices. Please log a Github issue if you need it.";
        public const string ErrorMessageReentry = "Reentry. This method is not thread safe";
        public const string ErrorMessageOperationNotSupportedOnPlatform = "You can't use this class on this platform";
        public const string InformationMessageFoundDevice = "Found device";
        public const string DebugMessageCompletionSourceNulled = "Completion source awaited and nulled";
        public const string InformationMessageDisposingDevice = "Disposing ... DeviceId: {deviceId}";
        #endregion

        #region IO
        public const string DebugMessageLockReleased = "Data received lock released. Completion source created. Waiting for data.";
        public static string GetErrorMessageInvalidWriteLength(int length, uint count) => $"Write failure. {length} bytes were sent to the device but it claims that {count} were sent.";
        public const string ErrorMessageReadWrite = "Read/Write Error DeviceId: {deviceId}";
        public const string WriteErrorMessage = "An error occurred while attempting to write to the device";
        public const string ErrorMessageRead = "An error occurred while attempting to read from the device";
        public const string ErrorMessageBufferSizeTooLarge = "The buffer size is too large";
        #endregion

        #region Polling
        public const string InformationMessageDeviceListenerPollingComplete = "Poll complete";
        public const string InformationMessageDeviceListenerDisconnected = "Disconnected";
        public const string ErrorMessagePollingError = "Hid polling error";
        public const string InformationMessageDeviceConnected = "Device connected. DeviceId: {deviceId}";
        public const string ErrorMessagePollingNotEnabled = "Polling is not enabled. Please specify pollMilliseconds in the constructor";
        #endregion

        #region Factories
        public const string ErrorMessageNoDeviceFactoriesRegistered = "No device factories have been registered";
        public const string ErrorMessageCouldntGetDevice = "Couldn't get device";
        #endregion

        #region USB
        public const string ErrorMessageReadEndpointNotRecognized = "The specified read endpoint is not of the correct type and cannot be used";
        public const string ErrorMessageWriteEndpointNotRecognized = "The specified write endpoint is not of the correct type and cannot be used";
        public const string ErrorMessageInvalidEndpoint = "This endpoint is not contained in the list of valid endpoints";
        public const string ErrorMessageInvalidInterface = "The interface is not contained the list of valid interfaces.";
        public const string ErrorMessageNoInterfaceFound = "There was no Usb Interface found for the device.";
        public const string MessageNoEndpointFound = "There was no endpoint found on the Usb interface";
        public const string ErrorMessageNoReadInterfaceSpecified = "There was no read Usb Interface specified for the device.";
        public const string WarningNoReadInterfaceFound = "There was no read Usb Interface found for the device. Attempting to use Interrupt interface instead";
        public const string WarningNoWriteInterfaceFound = "There was no write Usb Interface found for the device. Attempting to use Interrupt interface instead";
        public const string ErrorMessageNoWriteInterfaceSpecified = "There was no write Usb Interface specified for the device.";
        public const string WarningMessageWritingToInterrupt = "Writing to interrupt endpoint";

        public static string GetErrorMessageNoBulkPipe(byte interfaceNumber, bool isRead) => $"No bulk {(isRead ? "read" : "write")} pipes found. Interrupt pipes to be used instead. Interface Number {interfaceNumber}";
        #endregion

        #region Hid
        public const string ErrorMessagesCouldntGetHidString = "Could not get Hid string. Message: {message} Region: {region} Caller: {callMemberName}";
        #endregion

        #region Serial Port
        public const string ErrorCouldNotGetCommState = "Could not get Comm State";
        public const string ErrorCouldNotSetCommState = "Could not set Comm State";
        public const string ErrorCouldNotSetCommTimeout = "Could not set Comm Timeout";
        public const string ErrorMessageStopBitsMustBeSpecified = "Stop bits must be specified";
        public const string ErrorByteSizeMustBeFiveToEight = "Byte size must be between 5 and 8";
        public const string ErrorBaudRateInvalid = "Baud rate must be betweem 110 and 256000";
        public const string ErrorInvalidByteSizeAndStopBitsCombo = "The combination of byte size and stop bits is incorrect. 2 stop bits can't be used with 5 bytes, and byte sizes of more than five can't be used with stop bits of one point five.";
        public const string ErrorMessageOperationCanceled = "Operation was cancelled";
        #endregion
    }
}
