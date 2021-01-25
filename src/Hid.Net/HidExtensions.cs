using Device.Net;
using System;

namespace Hid.Net
{
    internal static class HidExtensions
    {
        /// <summary>
        /// Removes the first byte of the array and shifts other elements to the left
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static byte[] TrimFirstByte(this byte[] inputData)
        {
            var length = inputData.Length - 1;
            var data = new byte[length];
            Array.Copy(inputData, 1, data, 0, length);
            return data;
        }

        public static Report ToReadReport(this TransferResult tr)
        {
            //Grab the report id
            var reportId = tr.Data[0];

            //Create a new array and copy the data to it without the report id
            var data = tr.Data.TrimFirstByte();

            //Convert to a read report
            return new Report(reportId, new TransferResult(data, tr.BytesTransferred));
        }

        public static byte[] InsertReportIdAtIndexZero(this byte[] data, byte reportId)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var transformedData = InsertZeroAtIndexZero(data);

            //Set the report id at index 0
            transformedData[0] = reportId;

            return transformedData;
        }

        private static byte[] InsertZeroAtIndexZero(this byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            //Create a new array which is one byte larger 
            var transformedData = new byte[data.Length + 1];

            //copy the data to it without the report id at index 1
            Array.Copy(data, 0, transformedData, 1, data.Length);

            return transformedData;
        }

        /// <summary>
        /// Converts a Report to a Tranfer result and inserts the report Id at index 0
        /// </summary>
        /// <param name="readReport"></param>
        /// <returns></returns>
        public static TransferResult ToTransferResult(this Report readReport)
            => new TransferResult(
                InsertReportIdAtIndexZero(
                    readReport.TransferResult.Data,
                    readReport.ReportId), readReport.TransferResult.BytesTransferred);
    }
}
