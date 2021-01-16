using Device.Net;
using Device.Net.Exceptions;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Storage;
using hidDevice = Windows.Devices.HumanInterfaceDevice.HidDevice;

namespace Hid.Net.UWP
{
    ///<inheritdoc cref="IHidDevice"/>
    public class UWPHidDevice : UWPDeviceBase<hidDevice>, IHidDevice
    {
        #region Fields
        private bool disposed;
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        #endregion

        #region Public Properties
        public bool DataHasExtraByte { get; set; } = true;
        public byte? DefaultReportId { get; }
        #endregion

        #region Public Override Properties
        /// <summary>
        /// TODO: These vales are completely wrong and not being used anyway...
        /// </summary>
        public override ushort WriteBufferSize => 64;
        /// <summary>
        /// TODO: These vales are completely wrong and not being used anyway...
        /// </summary>
        public override ushort ReadBufferSize => 64;
        #endregion

        #region Event Handlers
        private void ConnectedDevice_InputReportReceived(hidDevice sender, HidInputReportReceivedEventArgs args)
        {
            Logger.LogDebug("Received Hid report Id: {id}", args?.Report?.Id);

            using var stream = args.Report.Data.AsStream();

            var bytes = new byte[args.Report.Data.Length];

            var bytesRead = (uint)stream.Read(bytes, 0, (int)args.Report.Data.Length);

            DataReceiver.DataReceived(new TransferResult(bytes, bytesRead));
        }
        #endregion

        #region Constructors
        public UWPHidDevice(
            ConnectedDeviceDefinition connectedDeviceDefinition,
            IDataReceiver dataReceiver,
            ILoggerFactory loggerFactory = null,
            byte? defaultReportId = null) : base(connectedDeviceDefinition.DeviceId, dataReceiver, loggerFactory)
        {
            ConnectedDeviceDefinition = connectedDeviceDefinition ?? throw new ArgumentNullException(nameof(connectedDeviceDefinition));
            DefaultReportId = defaultReportId;
        }
        #endregion

        #region Private Methods
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            //TODO: Put a lock here to stop reentrancy of multiple calls
            using var loggerScope = Logger?.BeginScope("DeviceId: {deviceId} Region: {region}", DeviceId, nameof(UWPHidDevice));

            Logger.LogInformation("Initializing Hid device {deviceId}", DeviceId);

            try
            {
                if (disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

                Logger.LogDebug(Messages.InformationMessageInitializingDevice);

                await GetDeviceAsync(DeviceId, cancellationToken);

                if (ConnectedDevice != null)
                {
                    ConnectedDevice.InputReportReceived += ConnectedDevice_InputReportReceived;
                }
                else
                {
                    throw new DeviceException($"The device {DeviceId} failed to initialize");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Messages.ErrorMessageCouldntIntializeDevice);
                throw;
            }
        }

        protected override IAsyncOperation<hidDevice> FromIdAsync(string id) => GetHidDevice(id);
        #endregion

        #region Public Methods
        public override void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, DeviceId);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, DeviceId);

            DataReceiver.Dispose();
            _WriteAndReadLock.Dispose();
            ConnectedDevice.InputReportReceived -= ConnectedDevice_InputReportReceived;

            base.Dispose();
        }

        public virtual Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default) => WriteReportAsync(data, DefaultReportId, cancellationToken);

        public async Task<uint> WriteReportAsync(byte[] data, byte? reportId, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (DataReceiver.HasData) Logger.LogWarning("Writing to device but data has already been received that has not been read");

            byte[] bytes;
            if (DataHasExtraByte)
            {
                bytes = new byte[data.Length + 1];
                Array.Copy(data, 0, bytes, 1, data.Length);
                bytes[0] = reportId.Value;
            }
            else
            {
                bytes = data;
            }

            var buffer = bytes.AsBuffer();
            var outReport = ConnectedDevice.CreateOutputReport();
            outReport.Data = buffer;

            try
            {
                var operation = ConnectedDevice.SendOutputReportAsync(outReport);
                var count = await operation.AsTask(cancellationToken);
                if (count == bytes.Length)
                {
                    Logger.LogDataTransfer(new Trace(true, bytes));
                }
                else
                {
                    Logger.LogError(Messages.GetErrorMessageInvalidWriteLength(bytes.Length, count) + "{length} {count}", bytes.Length, count, GetType().Name);
                    throw new IOException(Messages.GetErrorMessageInvalidWriteLength(bytes.Length, count));
                }

                return count;
            }
            catch (ArgumentException ex)
            {
                //TODO: Check the string is nasty. Validation on the size of the array being sent should be done earlier anyway
                if (string.Equals(ex.Message, "Value does not fall within the expected range.", StringComparison.Ordinal))
                {
                    throw new IOException("It seems that the data being sent to the device does not match the accepted size. Have you checked DataHasExtraByte?", ex);
                }
                throw;
            }
        }


        #endregion

        #region Public Overrides
        public async Task<ReadReport> ReadReportAsync(CancellationToken cancellationToken = default)
        {
            byte? reportId = null;
            var transferResult = await ReadAsync(cancellationToken);

            if (DataHasExtraByte)
            {
                reportId = transferResult.Data[0];
                transferResult = new TransferResult(DeviceBase.RemoveFirstByte(transferResult), transferResult.BytesTransferred);
            }

            return new ReadReport(reportId, new TransferResult(transferResult, transferResult.BytesTransferred));
        }

        public override async Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            var transferResult = await DataReceiver.ReadAsync(cancellationToken);
            Logger.LogDataTransfer(new Trace(false, transferResult));
            return transferResult;
        }
        #endregion

        #region Public Static Methods
        public static IAsyncOperation<hidDevice> GetHidDevice(string id) => hidDevice.FromIdAsync(id, FileAccessMode.ReadWrite);

        public async Task<TransferResult> WriteAndReadAsync(byte[] writeBuffer, CancellationToken cancellationToken = default)
        {
            await _WriteAndReadLock.WaitAsync(cancellationToken);

            using var logScope = Logger.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(WriteAndReadAsync));

            try
            {
                _ = await WriteAsync(writeBuffer, cancellationToken);
                var retVal = await ReadAsync(cancellationToken);

                Logger.LogDebug(Messages.SuccessMessageWriteAndReadCalled);
                return retVal;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Messages.ErrorMessageReadWrite);
                throw;
            }
            finally
            {
                _ = _WriteAndReadLock.Release();
            }
        }

        public Task Flush(CancellationToken cancellationToken = default) => throw new NotImplementedException(Messages.ErrorMessageFlushNotImplemented);
        #endregion
    }
}
