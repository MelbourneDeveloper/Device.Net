namespace Device.Net
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct ControlTransferResult
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        #region Public Properties
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        public uint BytesTransferred { get; }
        #endregion

        #region Conversion Operators
        public byte[] ToByteArray() => Data;

        #endregion

        #region Constructor
        public ControlTransferResult(
            uint bytesTransferred,
            byte[] data = null)
        {
            Data = data;
            BytesTransferred = bytesTransferred;
        }
        #endregion
    }
}