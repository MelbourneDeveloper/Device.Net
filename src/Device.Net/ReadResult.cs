using System;

namespace Device.Net
{
    public struct ReadResult
    {
        #region Public Properties
        public byte[] Data { get; }
        public int BytesRead { get; }
        #endregion

        #region Conversion Operators
        public static implicit operator byte[](ReadResult readResult)
        {
            return readResult.Data;
        }

        public static implicit operator ReadResult(byte[] data)
        {
            //TODO: This is a bit dodgy... It's breaking a code rule
            if (data == null) throw new ArgumentNullException(nameof(data));
            return new ReadResult(data, data.Length);
        }
        #endregion

        #region Constructor
        public ReadResult(byte[] data, int bytesRead)
        {
            Data = data;
            BytesRead = bytesRead;
        }
        #endregion
    }
}