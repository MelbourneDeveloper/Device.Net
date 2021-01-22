using Device.Net;

#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Hid.Net
{
    public struct ReadReport
    {
        public TransferResult TransferResult { get; }
        public byte ReportId { get; }

        public ReadReport(byte reportId, TransferResult transferResult)
        {
            TransferResult = transferResult;
            ReportId = reportId;
        }
    }
}
