using Device.Net;
using System;

namespace Hid.Net
{
    internal static class HidExtensions
    {
        private static byte[] TrimFirstByte(this TransferResult tr)
        {
            var length = tr.Data.Length - 1;
            var data = new byte[length];
            Array.Copy(tr.Data, 1, data, 0, length);
            return data;
        }

        public static ReadReport ToReadReport(this TransferResult tr)
        {
            //Grab the report id
            var reportId = tr.Data[0];

            //Create a new array and copy the data to it without the report id
            var data = tr.TrimFirstByte();

            //Convert to a read report
            return new ReadReport(reportId, new TransferResult(data, tr.BytesTransferred));
        }

        public static byte[] AddReportIdToIndexZero(this byte[] data, byte reportId)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            //Create a new array which is one byte larger 
            var transformedData = new byte[data.Length + 1];

            //Set the report id at index 0
            transformedData[0] = reportId;

            //copy the data to it without the report id at index 1
            Array.Copy(data, 0, transformedData, 1, data.Length);

            return transformedData;
        }

        public static TransferResult ToTransferResult(this ReadReport readReport)
        {
            var rawData = new byte[readReport.TransferResult.Data.Length + 1];

            Array.Copy(readReport.TransferResult.Data, 0, rawData, 1, readReport.TransferResult.Data.Length);

            return new TransferResult(rawData, readReport.TransferResult.BytesTransferred);
        }
    }
}
