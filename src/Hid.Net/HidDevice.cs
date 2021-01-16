using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net
{
    ///<inheritdoc cref="IHidDevice"/>
    public sealed class HidDevice : DeviceBase, IHidDevice
    {
        #region Private Fields

        private readonly IHidDeviceHandler _hidDeviceHandler;
        private bool _IsClosing;
        private bool disposed;

        #endregion Private Fields

        #region Public Constructors

        public HidDevice(
            IHidDeviceHandler hidDeviceHandler,
            ILoggerFactory loggerFactory = null,
            byte? defaultReportId = null) : base(
                hidDeviceHandler != null ? hidDeviceHandler.DeviceId : throw new ArgumentNullException(nameof(hidDeviceHandler)),
                loggerFactory,
                (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<HidDevice>())
        {
            _hidDeviceHandler = hidDeviceHandler;
            DefaultReportId = defaultReportId;
        }

        #endregion Public Constructors

        #region Public Properties

        public ConnectedDeviceDefinition ConnectedDeviceDefinition => _hidDeviceHandler.ConnectedDeviceDefinition;
        public byte? DefaultReportId { get; }
        public override bool IsInitialized => _hidDeviceHandler.IsInitialized;
        public bool? IsReadOnly => _hidDeviceHandler.IsReadOnly;
        public override ushort ReadBufferSize => _hidDeviceHandler.ReadBufferSize ?? throw new InvalidOperationException("Read buffer size unknown");
        public override ushort WriteBufferSize => _hidDeviceHandler.WriteBufferSize ?? throw new InvalidOperationException("Write buffer size unknown");

        #endregion Public Properties

        #region Private Properties

        private bool ReadBufferHasReportId => ReadBufferSize == 65;

        #endregion Private Properties

        #region Public Methods

        public void Close()
        {
            if (_IsClosing) return;

            _IsClosing = true;

            try
            {
                _hidDeviceHandler.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Messages.ErrorMessageCantClose, DeviceId, nameof(HidDevice));
            }

            _IsClosing = false;
        }

        public sealed override void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, DeviceId);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, DeviceId);

            GC.SuppressFinalize(this);

            Close();

            base.Dispose();
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

            using var logScope = Logger.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(InitializeAsync));

            try
            {
                return _hidDeviceHandler.InitializeAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Messages.ErrorMessageCouldntIntializeDevice);
                throw;
            }
        }

        public override async Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            var readReport = await ReadReportAsync(cancellationToken).ConfigureAwait(false);
            Logger.LogDataTransfer(new Trace(false, readReport.Data));
            return readReport.Data;
        }

        public async Task<ReadReport> ReadReportAsync(CancellationToken cancellationToken = default)
        {
            byte? reportId = null;
            byte[] bytes;
            TransferResult actualTransferResult;

            try
            {
                actualTransferResult = await _hidDeviceHandler.ReadAsync(cancellationToken).ConfigureAwait(false);
                bytes = actualTransferResult.Data;
            }
            catch (OperationCanceledException oce)
            {
                Logger.LogError(oce, Messages.ErrorMessageOperationCanceled);
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Messages.ErrorMessageRead);
                throw new IOException(Messages.ErrorMessageRead, ex);
            }

            if (ReadBufferHasReportId) reportId = bytes.First();

            var retVal = ReadBufferHasReportId ? RemoveFirstByte(bytes) : bytes;

            return new ReadReport(reportId, new TransferResult(retVal, actualTransferResult.BytesTransferred));
        }

        public override Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default) => WriteReportAsync(data, DefaultReportId, cancellationToken);

        public async Task<uint> WriteReportAsync(byte[] data, byte? reportId, CancellationToken cancellationToken = default)
        {
            using var logScope = Logger.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(WriteReportAsync));

            try
            {
                uint bytesWritten = 0;

                if (IsReadOnly.HasValue && IsReadOnly.Value)
                {
                    throw new ValidationException("This device was opened in Read Only mode.");
                }

                if (data == null) throw new ArgumentNullException(nameof(data));

                byte[] bytes;
                if (reportId.HasValue)
                {
                    //Copy the data to a new array that is one byte larger and shif the data to the right by 1
                    bytes = new byte[WriteBufferSize];
                    Array.Copy(data, 0, bytes, 1, data.Length);
                    //Put the report Id at the first index
                    bytes[0] = reportId.Value;
                }
                else
                {
                    bytes = data;
                }

                try
                {
                    bytesWritten = await _hidDeviceHandler.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
                    Logger.LogDataTransfer(new Trace(true, bytes));
                }
                catch (Exception ex)
                {
                    throw new IOException(Messages.WriteErrorMessage, ex);
                }

                return bytesWritten;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, Messages.WriteErrorMessage);
                throw;
            }
        }

        #endregion Public Methods

    }
}