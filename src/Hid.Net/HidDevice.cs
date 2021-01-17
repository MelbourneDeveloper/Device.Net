using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
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
            byte? defaultWriteReportId = 0
            ) :
            base(
                hidDeviceHandler != null ? hidDeviceHandler.DeviceId : throw new ArgumentNullException(nameof(hidDeviceHandler)),
                loggerFactory,
                (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<HidDevice>())
        {
            _hidDeviceHandler = hidDeviceHandler;
            DefaultWriteReportId = defaultWriteReportId;
        }

        #endregion Public Constructors

        #region Public Properties

        public ConnectedDeviceDefinition ConnectedDeviceDefinition => _hidDeviceHandler.ConnectedDeviceDefinition;
        public bool IsInitialized => _hidDeviceHandler.IsInitialized;
        public bool? IsReadOnly => _hidDeviceHandler.IsReadOnly;
        public ushort ReadBufferSize => _hidDeviceHandler.ReadBufferSize ?? throw new InvalidOperationException("Read buffer size unknown");
        public ushort WriteBufferSize => _hidDeviceHandler.WriteBufferSize ?? throw new InvalidOperationException("Write buffer size unknown");
        public byte? DefaultWriteReportId { get; }

        #endregion Public Properties

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
            try
            {
                return await _hidDeviceHandler.ReadReportAsync(cancellationToken).ConfigureAwait(false);
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
        }

        /// <summary>
        /// Write a report. The report Id comes from DefaultReportId, or the first byte in the array if the DefaultReportId is null
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
            =>
            //Validate
            data == null || data.Length == 0 ? throw new InvalidOperationException("You must specify a default Report Id, or pass the Report Id as the first byte in the array")
            :
            //Write a report based on the default report id or the first byte in the array
            WriteReportAsync(data, DefaultWriteReportId ?? data[0], cancellationToken);


        public async Task<uint> WriteReportAsync(byte[] data, byte reportId, CancellationToken cancellationToken = default)
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

                try
                {
                    bytesWritten = await _hidDeviceHandler.WriteReportAsync(data, reportId, cancellationToken).ConfigureAwait(false);
                    Logger.LogDataTransfer(new Trace(true, data));
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