using Device.Net;

#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Hid.Net
{
    /// <summary>
    /// Represents a Hid Input or Output report. It consists of a TransferResult to/from the device, and a Report Id. An output report is for writing to the device, and input report is for reading from the device.
    /// </summary>
    public struct Report
    {
        /// <summary>
        /// Data Transferred to/from the device
        /// </summary>
        public TransferResult TransferResult { get; }

        /// <summary>
        /// The Hid report Id
        /// </summary>
        public byte ReportId { get; }

        /// <summary>
        /// Constructs a report
        /// </summary>
        public Report(byte reportId, TransferResult transferResult)
        {
            TransferResult = transferResult;
            ReportId = reportId;
        }
    }
}
