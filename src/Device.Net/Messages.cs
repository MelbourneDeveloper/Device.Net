namespace Device.Net
{
    public static class Messages
    {
        public static string SuccessMessageWriteAndReadCalled => $"Successfully called {nameof(DeviceBase.WriteAndReadAsync)}";
    }
}
