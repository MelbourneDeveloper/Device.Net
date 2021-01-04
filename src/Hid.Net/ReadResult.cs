using Device.Net;

namespace Hid.Net
{
    public class ReadReport
    {
        public TransferResult Data { get; }
        public byte? ReportId { get; }

        public ReadReport(byte? reportId, TransferResult transferResult)
        {
            Data = transferResult;
            ReportId = reportId;
        }
    }
}
