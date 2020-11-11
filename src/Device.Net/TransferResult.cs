using System;

namespace Device.Net
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct TransferResult

#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        #region Public Properties
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        //TODO: Int or Uint?
        public uint BytesTransferred { get; }
        #endregion

        #region Conversion Operators
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator byte[](TransferResult TransferResult) => TransferResult.Data;

        public static implicit operator TransferResult(byte[] data) =>
            //TODO: This is a bit dodgy... It's breaking a code rule
            data == null ? throw new ArgumentNullException(nameof(data)) : new TransferResult(data, (uint)data.Length);
#pragma warning restore CA2225 // Operator overloads have named alternates
        #endregion

        #region Constructor
        public TransferResult(byte[] data, uint bytesRead)
        {
            Data = data;
            BytesTransferred = bytesRead;
        }
        #endregion
    }
}