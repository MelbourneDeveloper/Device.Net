namespace Device.Net
{
    public static class Messages
    {
        public const string ErrorMessageNotInitialized = "The device has not been initialized.";

        public static string SuccessMessageWriteAndReadCalled => $"Successfully called {nameof(DeviceBase.WriteAndReadAsync)}";

        public static string GetErrorMessageInvalidWriteLength(int length, uint count)
        {
            return $"Write failure. {length} bytes were sent to the device but it claims that {count} were sent.";
        }
    }
}
