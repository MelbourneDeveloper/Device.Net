using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public sealed class WindowsHidDevice : WindowsDeviceBase, IHidDevice
    {
        #region Fields
        private Stream _ReadFileStream;
        private Stream _WriteFileStream;
        private SafeFileHandle _ReadSafeFileHandle;
        private SafeFileHandle _WriteSafeFileHandle;
        private bool _IsClosing;
        private bool disposed;
        private readonly ushort? _WriteBufferSize;
        private readonly ushort? _ReadBufferSize;

        #endregion

        #region Private Properties
        private bool ReadBufferHasReportId => ReadBufferSize == 65;
        #endregion

        #region Protected Properties
        protected override string LogSection => nameof(WindowsHidDevice);
        #endregion

        #region Public Overrides
        public override bool IsInitialized => _ReadSafeFileHandle != null && !_ReadSafeFileHandle.IsInvalid;
        public override ushort WriteBufferSize => _WriteBufferSize ?? (ConnectedDeviceDefinition == null ? (ushort)0 : (ushort)ConnectedDeviceDefinition.WriteBufferSize.Value);
        public override ushort ReadBufferSize => _ReadBufferSize ?? (ConnectedDeviceDefinition == null ? (ushort)0 : (ushort)ConnectedDeviceDefinition.ReadBufferSize.Value);
        public bool? IsReadOnly { get; private set; }
        #endregion

        #region Public Properties
        public byte DefaultReportId { get; set; }
        public IHidApiService HidService { get; }
        #endregion

        #region Constructor
        public WindowsHidDevice(string deviceId) : this(deviceId, null, null, null, null)
        {
        }

        public WindowsHidDevice(string deviceId, ILogger logger, ITracer tracer) : this(deviceId, null, null, logger, tracer)
        {
        }

        public WindowsHidDevice(string deviceId, ushort? writeBufferSize, ushort? readBufferSize, ILogger logger, ITracer tracer) : this(deviceId, writeBufferSize, readBufferSize, logger, tracer, null)
        {

        }

        public WindowsHidDevice(string deviceId, ushort? writeBufferSize, ushort? readBufferSize, ILogger logger, ITracer tracer, IHidApiService hidService) : base(deviceId, logger, tracer)
        {
            _WriteBufferSize = writeBufferSize;
            _ReadBufferSize = readBufferSize;
            HidService = hidService ?? new WindowsHidApiService(logger);
        }
        #endregion

        #region Private Methods
        private bool Initialize()
        {
            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(Initialize));

                Close();

                if (string.IsNullOrEmpty(DeviceId))
                {
                    throw new ValidationException($"{nameof(DeviceId)} must be specified before {nameof(Initialize)} can be called.");
                }

                _ReadSafeFileHandle = HidService.CreateReadConnection(DeviceId, FileAccessRights.GenericRead);
                _WriteSafeFileHandle = HidService.CreateWriteConnection(DeviceId);

                if (_ReadSafeFileHandle.IsInvalid)
                {
                    throw new ApiException(Messages.ErrorMessageCantOpenRead);
                }

                IsReadOnly = _WriteSafeFileHandle.IsInvalid;

                if (IsReadOnly.Value)
                {
                    Logger?.LogWarning(Messages.WarningMessageOpeningInReadonlyMode, DeviceId);
                }

                ConnectedDeviceDefinition = HidService.GetDeviceDefinition(DeviceId, _ReadSafeFileHandle);

                var readBufferSize = ReadBufferSize;
                var writeBufferSize = WriteBufferSize;

                if (readBufferSize == 0)
                {
                    throw new ValidationException($"{nameof(ReadBufferSize)} must be specified. HidD_GetAttributes may have failed or returned an InputReportByteLength of 0. Please specify this argument in the constructor");
                }

                _ReadFileStream = HidService.OpenRead(_ReadSafeFileHandle, readBufferSize);

                if (_ReadFileStream.CanRead)
                {
                    Logger?.LogInformation(Messages.SuccessMessageReadFileStreamOpened);
                }
                else
                {
                    Logger?.LogWarning(Messages.WarningMessageReadFileStreamCantRead);
                }

                if (!IsReadOnly.Value)
                {
                    if (writeBufferSize == 0)
                    {
                        throw new ValidationException($"{nameof(WriteBufferSize)} must be specified. HidD_GetAttributes may have failed or returned an OutputReportByteLength of 0. Please specify this argument in the constructor");
                    }

                    //Don't open if this is a read only connection
                    _WriteFileStream = HidService.OpenWrite(_WriteSafeFileHandle, writeBufferSize);

                    if (_WriteFileStream.CanWrite)
                    {
                        Logger?.LogInformation(Messages.SuccessMessageWriteFileStreamOpened);
                    }
                    else
                    {
                        Logger?.LogWarning(Messages.WarningMessageWriteFileStreamCantWrite);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageCouldntIntializeDevice);
                throw;
            }
            finally
            {
                logScope?.Dispose();
            }

            return true;
        }
        #endregion

        #region Public Methods
        public void Close()
        {
            if (_IsClosing) return;

            _IsClosing = true;

            try
            {
                _ReadFileStream?.Dispose();
                _WriteFileStream?.Dispose();

                _ReadFileStream = null;
                _WriteFileStream = null;

                if (_ReadSafeFileHandle != null)
                {
                    _ReadSafeFileHandle.Dispose();
                    _ReadSafeFileHandle = null;
                }

                if (_WriteSafeFileHandle != null)
                {
                    _WriteSafeFileHandle.Dispose();
                    _WriteSafeFileHandle = null;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageCantClose, DeviceId, nameof(WindowsHidDevice));
            }

            _IsClosing = false;
        }

        public override void Dispose()
        {
            if (disposed) return;

            GC.SuppressFinalize(this);

            disposed = true;

            Close();

            base.Dispose();
        }

        public override async Task InitializeAsync()
        {
            if (disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

            await Task.Run(() => Initialize());
        }

        public override async Task<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            var data = (await ReadReportAsync(cancellationToken)).Data;
            Tracer?.Trace(false, data);
            return data;
        }

        public async Task<ReadReport> ReadReportAsync(CancellationToken cancellationToken = default)
        {
            byte? reportId = null;

            if (_ReadFileStream == null)
            {
                throw new NotInitializedException(Messages.ErrorMessageNotInitialized);
            }

            var bytes = new byte[ReadBufferSize];

            try
            {
                await _ReadFileStream.ReadAsync(bytes, 0, bytes.Length, cancellationToken);
            }
            catch (OperationCanceledException oce)
            {
                Logger?.LogError(oce, Messages.ErrorMessageOperationCanceled);
                throw;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageRead);
                throw new IOException(Messages.ErrorMessageRead, ex);
            }

            if (ReadBufferHasReportId) reportId = bytes.First();

            var retVal = ReadBufferHasReportId ? RemoveFirstByte(bytes) : bytes;

            return new ReadReport(reportId, retVal);
        }

        public override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default) => WriteReportAsync(data, null, cancellationToken);

        public async Task WriteReportAsync(byte[] data, byte? reportId, CancellationToken cancellationToken = default)
        {
            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(WriteReportAsync));

                if (IsReadOnly.HasValue && IsReadOnly.Value)
                {
                    throw new ValidationException($"This device was opened in Read Only mode.");
                }

                if (data == null) throw new ArgumentNullException(nameof(data));

                if (_WriteFileStream == null)
                {
                    throw new NotInitializedException("The device has not been initialized");
                }

                byte[] bytes;
                if (reportId.HasValue)
                {
                    //Copy the data to a new array that is one byte larger and shif the data to the right by 1
                    bytes = new byte[WriteBufferSize + 1];
                    Array.Copy(data, 0, bytes, 1, data.Length);
                    //Put the report Id at the first index
                    bytes[0] = reportId ?? DefaultReportId;
                }
                else
                {
                    bytes = data;
                }

                if (_WriteFileStream.CanWrite)
                {
                    try
                    {
                        await _WriteFileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                        Tracer?.Trace(true, bytes);
                    }
                    catch (Exception ex)
                    {
                        throw new IOException(Messages.WriteErrorMessage, ex);
                    }
                }
                else
                {
                    throw new IOException("The file stream cannot be written to");
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.WriteErrorMessage);
                throw;
            }
            finally
            {
                logScope?.Dispose();
            }
        }
        #endregion

        #region Finalizer
        ~WindowsHidDevice()
        {
            Dispose();
        }
        #endregion
    }
}