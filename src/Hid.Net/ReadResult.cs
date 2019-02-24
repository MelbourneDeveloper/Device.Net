namespace Hid.Net
{
    public class ReadReport
    {
        public byte[] Data { get; }
        public byte? ReportId { get; }

        public ReadReport(byte? reportId, byte[] data)
        {
            Data = data;
            ReportId = reportId;
        }
    }
}
