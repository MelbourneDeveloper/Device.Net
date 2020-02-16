using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
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
            try
            {
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
                    Logger?.Log(Messages.WarningMessageOpeningInReadonlyMode(DeviceId), nameof(WindowsHidDevice), null, LogLevel.Warning);
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
                    Logger?.Log(Messages.SuccessMessageReadFileStreamOpened, nameof(WindowsHidDevice), null, LogLevel.Information);
                }
                else
                {
                    Logger?.Log(Messages.WarningMessageReadFileStreamCantRead, nameof(WindowsHidDevice), null, LogLevel.Warning);
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
                        Logger?.Log(Messages.SuccessMessageWriteFileStreamOpened, nameof(WindowsHidDevice), null, LogLevel.Information);
                    }
                    else
                    {
                        Logger?.Log(Messages.WarningMessageWriteFileStreamCantWrite, nameof(WindowsHidDevice), null, LogLevel.Warning);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger?.Log($"{nameof(Initialize)} error.", nameof(WindowsHidDevice), ex, LogLevel.Error);
                throw;
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
            catch (Exception)
            {
                //TODO: Logging
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
            catch(OperationCanceledException oce)
            {
                Log(Messages.ErrorMessageOperationCanceled, oce);
                throw;
            }
            catch (Exception ex)
            {
                Log(Messages.ErrorMessageRead, ex);
                throw new IOException(Messages.ErrorMessageRead, ex);
            }

            if (ReadBufferHasReportId) reportId = bytes.First();

            var retVal = ReadBufferHasReportId ? RemoveFirstByte(bytes) : bytes;

            return new ReadReport(reportId, retVal);
        }

        private void Log(object errorMessageOperationCanceled, OperationCanceledException oce)
        {
            throw new NotImplementedException();
        }

        public override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            return WriteReportAsync(data, 0, cancellationToken);
        }

        public async Task WriteReportAsync(byte[] data, byte? reportId, CancellationToken cancellationToken = default)
        {
            if (IsReadOnly.HasValue && IsReadOnly.Value)
            {
                throw new ValidationException($"This device was opened in Read Only mode.");
            }

            if (data == null) throw new ArgumentNullException(nameof(data));

            if (_WriteFileStream == null)
            {
                throw new NotInitializedException("The device has not been initialized");
            }

            if (data.Length > WriteBufferSize)
            {
                throw new ValidationException($"Data is longer than {WriteBufferSize - 1} bytes which is the device's OutputReportByteLength.");
            }

            byte[] bytes;
            if (WriteBufferSize == 65)
            {
                if (WriteBufferSize == data.Length)
                {
                    throw new DeviceException("The data sent to the device was a the same length as the HidCollectionCapabilities.OutputReportByteLength. This probably indicates that DataHasExtraByte should be set to false.");
                }

                bytes = new byte[WriteBufferSize];
                Array.Copy(data, 0, bytes, 1, data.Length);
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
                    Log(Messages.WriteErrorMessage, ex);
                    throw new IOException(Messages.WriteErrorMessage, ex);
                }
            }
            else
            {
                throw new IOException("The file stream cannot be written to");
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