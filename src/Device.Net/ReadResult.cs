namespace Device.Net
{
    public struct ReadResult
    {
        #region Public Properties
        public byte[] Data { get; }


        //TODO: Int or Uint?
        public uint BytesRead { get; }
        #endregion

        #region Conversion Operators
        public static implicit operator byte[] (ReadResult readResult)
        {
            return readResult.Data;
        }

        public static implicit operator ReadResult(byte[] data)
        {
            return new ReadResult(data, (uint)(data != null ? data.Length : 0));
        }
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