#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Device.Net
{
    /// <summary>
    /// Represents the result of a read or write transfer
    /// </summary>
    public readonly struct TransferResult
    {
        #region Public Properties
        /// <summary>
        /// The data that was transferred
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// The number of bytes transferred
        /// </summary>
        public uint BytesTransferred { get; }
        #endregion

        #region Conversion Operators
        public static implicit operator byte[](TransferResult TransferResult) => TransferResult.Data;

        /// <summary>
        /// This automatically converts an array of bytes to <see cref="TransferResult"/>. TODO: Remove this because it is too easy to swallow up the information of how many bytes were actually read
        /// </summary>
        /// <param name="data"></param>
        public static implicit operator TransferResult(byte[] data) =>
            new TransferResult(data, data != null ? (uint)data.Length : 0);
        #endregion

        #region Constructor
        public TransferResult(byte[] data, uint bytesRead)
        {
            Data = data;
            BytesTransferred = bytesRead;
        }
        #endregion

        public override string ToString() => $"Bytes transferred: {BytesTransferred}\r\n{string.Join(", ", Data)}";
    }
}