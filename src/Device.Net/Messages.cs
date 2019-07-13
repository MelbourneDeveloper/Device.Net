namespace Device.Net
{
    public static class Messages
    {
        #region Device Initialization
        public const string ErrorMessageNotInitialized = "The device has not been initialized.";
        public const string ErrorMessageCouldntIntializeDevice = "Couldn't initialize device";
        public const string ErrorMessageCantOpenWrite = "Could not open connection for writing";
        public const string ErrorMessageCantOpenRead = "Could not open connection for reading";
        #endregion

        #region Misc
        public const string ErrorMessageReentry = "Reentry. This method is not thread safe";
        public static string SuccessMessageWriteAndReadCalled => $"Successfully called {nameof(DeviceBase.WriteAndReadAsync)}";
        #endregion

        #region IO
        public static string GetErrorMessageInvalidWriteLength(int length, uint count)
        {
            return $"Write failure. {length} bytes were sent to the device but it claims that {count} were sent.";
        }

        public const string ErrorMessageReadWrite = "Read/Write Error";
        #endregion

        #region Polling
        public const string InformationMessageDeviceListenerPollingComplete = "Poll complete";
        public const string InformationMessageDeviceListenerDisconnected = "Disconnected";
        public const string ErrorMessagePollingError = "Hid polling error";
        public const string InformationMessageDeviceConnected = "Device connected";
        public const string ErrorMessagePollingNotEnabled = "Polling is not enabled. Please specify pollMilliseconds in the constructor";
        #endregion

        #region Factories
        public const string ErrorMessageNoDeviceFactoriesRegistered = "No device factories have been registered";
        public const string ErrorMessageCouldntGetDevice = "Couldn't get a device";
        #endregion

        #region USB
        public const string ErrorMessageInvalidEndpoint = "This endpoint is not contained in the list of valid endpoints";
        #endregion

    }
}
