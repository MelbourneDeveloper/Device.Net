﻿using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public delegate SafeFileHandle CreateConnection(
        IApiService apiService,
        string deviceId,
        FileAccessRights desiredAccess,
        uint shareMode,
        uint creationDisposition);

    internal class WindowsHidHandler : IHidDeviceHandler
    {
        #region Private Fields

        private readonly IHidApiService _hidService;
        private readonly ILogger _logger;
        private readonly Func<TransferResult, Report> _readTransferTransform;
        private readonly Func<byte[], byte, byte[]> _writeTransferTransform;
        private Stream _readFileStream;
        private SafeFileHandle _readSafeFileHandle;
        private Stream _writeFileStream;
        private SafeFileHandle _writeSafeFileHandle;
        private readonly CreateConnection _createReadConnection;
        private readonly CreateConnection _createWriteConnection;

        #endregion Private Fields

        #region Public Constructors

        public WindowsHidHandler(
            string deviceId,
            ushort? writeBufferSize = null,
            ushort? readBufferSize = null,
            IHidApiService hidApiService = null,
            ILoggerFactory loggerFactory = null,
            Func<TransferResult, Report> readTransferTransform = null,
            Func<byte[], byte, byte[]> writeTransferTransform = null,
            CreateConnection createReadConnection = null,
            CreateConnection createWriteConnection = null)
        {
            _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<WindowsHidHandler>();
            DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));

            _readTransferTransform = readTransferTransform ??
                new Func<TransferResult, Report>((tr) => tr.ToReadReport(_logger));

            _writeTransferTransform = writeTransferTransform ??
                new Func<byte[], byte, byte[]>(
                (data, reportId) => data.InsertReportIdAtIndexZero(reportId, _logger));

            _hidService = hidApiService ?? new WindowsHidApiService(loggerFactory);
            WriteBufferSize = writeBufferSize;
            ReadBufferSize = readBufferSize;

            _createReadConnection = createReadConnection ??= (apiService, deviceId, fileAccessRights, shareMode, creationDisposition)
                => apiService.CreateReadConnection(deviceId, fileAccessRights);

            _createWriteConnection = createWriteConnection ??= (apiService, deviceId, fileAccessRights, shareMode, creationDisposition)
                => apiService.CreateWriteConnection(deviceId);
        }

        #endregion Public Constructors

        #region Public Properties

        public ConnectedDeviceDefinition ConnectedDeviceDefinition { get; private set; }
        public string DeviceId { get; }
        public bool IsInitialized { get; private set; }
        public bool? IsReadOnly { get; private set; }
        public ushort? ReadBufferSize { get; private set; }
        public ushort? WriteBufferSize { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Close()
        {
            _readFileStream?.Dispose();
            _writeFileStream?.Dispose();

            _readFileStream = null;
            _writeFileStream = null;

            if (_readSafeFileHandle != null)
            {
                _readSafeFileHandle.Dispose();
                _readSafeFileHandle = null;
            }

            if (_writeSafeFileHandle != null)
            {
                _writeSafeFileHandle.Dispose();
                _writeSafeFileHandle = null;
            }
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
              {
                  using var logScope = _logger.BeginScope("DeviceId: {deviceId} Call: {call}", DeviceId, nameof(InitializeAsync));

                  if (string.IsNullOrEmpty(DeviceId))
                  {
                      throw new ValidationException(
                          $"{nameof(DeviceId)} must be specified before {nameof(InitializeAsync)} can be called.");
                  }

                  _readSafeFileHandle = _hidService.CreateReadConnection(DeviceId, FileAccessRights.GenericRead);
                  _writeSafeFileHandle = _hidService.CreateWriteConnection(DeviceId);

                  if (_readSafeFileHandle.IsInvalid)
                  {
                      throw new ApiException(Messages.ErrorMessageCantOpenRead);
                  }

                  IsReadOnly = _writeSafeFileHandle.IsInvalid;

                  if (IsReadOnly.Value)
                  {
                      _logger.LogWarning(Messages.WarningMessageOpeningInReadonlyMode, DeviceId);
                  }

                  ConnectedDeviceDefinition = _hidService.GetDeviceDefinition(DeviceId, _readSafeFileHandle);

                  ReadBufferSize ??= (ushort?)ConnectedDeviceDefinition.ReadBufferSize;
                  WriteBufferSize ??= (ushort?)ConnectedDeviceDefinition.WriteBufferSize;

                  if (!ReadBufferSize.HasValue)
                  {
                      throw new ValidationException(
                          $"ReadBufferSize must be specified. HidD_GetAttributes may have failed or returned an InputReportByteLength of 0. Please specify this argument in the constructor");
                  }

                  _readFileStream = _hidService.OpenRead(_readSafeFileHandle, ReadBufferSize.Value);

                  if (_readFileStream.CanRead)
                  {
                      _logger.LogInformation(Messages.SuccessMessageReadFileStreamOpened);
                  }
                  else
                  {
                      _logger.LogWarning(Messages.WarningMessageReadFileStreamCantRead);
                  }

                  if (IsReadOnly.Value) return;

                  if (!WriteBufferSize.HasValue)
                  {
                      throw new ValidationException(
                          $"WriteBufferSize must be specified. HidD_GetAttributes may have failed or returned an OutputReportByteLength of 0. Please specify this argument in the constructor");
                  }

                  //Don't open if this is a read only connection
                  _writeFileStream = _hidService.OpenWrite(_writeSafeFileHandle, WriteBufferSize.Value);

                  if (_writeFileStream.CanWrite)
                  {
                      _logger.LogInformation(Messages.SuccessMessageWriteFileStreamOpened);
                  }
                  else
                  {
                      _logger.LogWarning(Messages.WarningMessageWriteFileStreamCantWrite);
                  }

                  IsInitialized = true;
              }, cancellationToken);
        }

        public async Task<Report> ReadReportAsync(CancellationToken cancellationToken = default)
        {
            if (_readFileStream == null)
            {
                throw new NotInitializedException(Messages.ErrorMessageNotInitialized);
            }

            //Read the data
            var bytes = new byte[ReadBufferSize.Value];
            var bytesRead = (uint)await _readFileStream.ReadAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);

            var transferResult = new TransferResult(bytes, bytesRead);

            //Log the data read
            _logger.LogDataTransfer(new Trace(false, transferResult));

            //Transform to a ReadReport
            return _readTransferTransform(transferResult);
        }

        public async Task<uint> WriteReportAsync(byte[] data, byte reportId, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (_writeFileStream == null)
            {
                throw new NotInitializedException("The device has not been initialized");
            }

            if (_writeFileStream.CanWrite)
            {
                var transformedData = _writeTransferTransform(data, reportId);
                await _writeFileStream.WriteAsync(transformedData, 0, transformedData.Length, cancellationToken).ConfigureAwait(false);
                _logger.LogDataTransfer(new Trace(true, transformedData));
                return (uint)data.Length;
            }
            else
            {
                throw new IOException("The file stream cannot be written to");
            }
        }

        #endregion Public Methods
    }
}