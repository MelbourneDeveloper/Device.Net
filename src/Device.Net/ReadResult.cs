using System;

namespace Device.Net
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct ReadResult
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        #region Public Properties
        public byte[] Data { get; }


        //TODO: Int or Uint?
        public uint BytesRead { get; }
        #endregion

        #region Conversion Operators
        public static implicit operator byte[](ReadResult readResult) => readResult.Data;

        public static implicit operator ReadResult(byte[] data) =>
            //TODO: This is a bit dodgy... It's breaking a code rule
            data == null ? throw new ArgumentNullException(nameof(data)) : new ReadResult(data, (uint)data.Length);
        #endregion

        #region Constructor
        public ReadResult(byte[] data, uint bytesRead)
        {
            Data = data;
            BytesRead = bytesRead;
        }
        #endregion
    }
}