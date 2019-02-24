namespace Hid.Net
{
    public class ReadResult
    {
        public byte[] Data { get; }
        public byte? ReportId { get; }

        public ReadResult(byte? reportId, byte[] data)
        {
            Data = data;
            ReportId = reportId;
        }
    }
}
