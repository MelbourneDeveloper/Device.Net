using Device.Net;
using Microsoft.Extensions.Logging;
using System;

namespace Hid.Net
{
    internal static class HidExtensions
    {
        #region Public Methods

        /// <summary>
        /// Shifts the array to the right and inserts the report id at index zero
        /// </summary>
        public static byte[] InsertReportIdAtIndexZero(this byte[] data, byte reportId, ILogger logger)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var transformedData = InsertZeroAtIndexZero(data);

            //Set the report id at index 0
            transformedData[0] = reportId;

            logger.LogDebug("Shifted data one place to the right and inserted {reportId} at index zero. Input Length: {inputLength} Output Length: {outputLength}", reportId, data.Length, transformedData.Length);

            return transformedData;
        }

        public static Report ToReadReport(this TransferResult tr, ILogger logger)
        {
            //Grab the report id
            var reportId = tr.Data[0];

            logger.LogDebug("Got the report id {reportId} from transfer result at index zero", reportId);

            //Create a new array and copy the data to it without the report id
            var data = tr.Data.TrimFirstByte(logger);

            logger.LogDebug("Returning the report based on the data and the Report Id", reportId);

            //Convert to a read report
            return new Report(reportId, new TransferResult(data, tr.BytesTransferred));
        }

        /// <summary>
        /// Converts a Report to a Tranfer result and inserts the report Id at index 0
        /// </summary>
        public static TransferResult ToTransferResult(this Report readReport, ILogger logger)
            => new TransferResult(
                InsertReportIdAtIndexZero(
                    readReport.TransferResult.Data,
                    readReport.ReportId,
                    logger), readReport.TransferResult.BytesTransferred);

        /// <summary>
        /// Removes the first byte of the array and shifts other elements to the left
        /// </summary>
        public static byte[] TrimFirstByte(this byte[] inputData, ILogger logger)
        {
            var length = inputData.Length - 1;
            var data = new byte[length];
            Array.Copy(inputData, 1, data, 0, length);

            logger.LogDebug("Removed byte at index zero and shifted the array to the left by one place. Input Length: {inputLength} Output Length: {outputLength}", inputData.Length, data.Length);

            return data;
        }

        #endregion Public Methods

        #region Private Methods

        private static byte[] InsertZeroAtIndexZero(this byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            //Create a new array which is one byte larger 
            var transformedData = new byte[data.Length + 1];

            //copy the data to it without the report id at index 1
            Array.Copy(data, 0, transformedData, 1, data.Length);

            return transformedData;
        }

        #endregion Private Methods
    }
}
