namespace Device.Net
{
    public struct ReadResult
    {
        public static implicit operator byte[](ReadResult readResult)
        {
            return readResult.Data;
        }

        //public static explicit operator byte[](byte[] data)
        //{
        //    if (data == null) throw new ArgumentNullException(nameof(data));

        //    return new ReadResult(data, data.Length);
        //}

        public ReadResult(byte[] data, int bytesRead)
        {
            Data = data;
            BytesRead = bytesRead;
        }

        public byte[] Data { get; }
        public int BytesRead { get; }

        public byte[] To()
        {
            return Data;
        }
    }
}