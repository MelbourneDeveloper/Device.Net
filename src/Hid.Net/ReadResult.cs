namespace Hid.Net
{
    public class ReadReport
    {
        public byte[] Data { get; }
        public byte? ReportId { get; }
        public uint BytesRead { get; }

        public ReadReport(byte? reportId, byte[] data, uint bytesRead)
        {
            Data = data;
            ReportId = reportId;
            BytesRead = bytesRead;
        }
    }
}
