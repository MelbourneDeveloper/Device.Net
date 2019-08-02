using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public sealed class WindowsHidDevice : WindowsDeviceBase, IHidDevice
    {
        #region Fields
        private FileStream _ReadFileStream;
        private FileStream _WriteFileStream;
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
        public override bool IsInitialized => _WriteSafeFileHandle != null && !_WriteSafeFileHandle.IsInvalid;
        public override ushort WriteBufferSize => _WriteBufferSize ?? (ConnectedDeviceDefinition == null ? (ushort)0 : (ushort)ConnectedDeviceDefinition.WriteBufferSize.Value);
        public override ushort ReadBufferSize => _ReadBufferSize ?? (ConnectedDeviceDefinition == null ? (ushort)0 : (ushort)ConnectedDeviceDefinition.ReadBufferSize.Value);
        public bool? IsReadOnly { get; private set; }
        #endregion

        #region Public Properties
        public byte DefaultReportId { get; set; }
        #endregion

        #region Constructor
        public WindowsHidDevice(string deviceId) : this(deviceId, null, null, null, null)
        {
        }

        public WindowsHidDevice(string deviceId, ILogger logger, ITracer tracer) : this(deviceId, null, null, logger, tracer)
        {
        }

        public WindowsHidDevice(string deviceId, ushort? writeBufferSize, ushort? readBufferSize, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
            _WriteBufferSize = writeBufferSize;
            _ReadBufferSize = readBufferSize;
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

                //TODO: Work on getting these correct, and make sure that different values can be passed in here.
                _ReadSafeFileHandle = APICalls.CreateFile(DeviceId, APICalls.GenericRead, 3, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);
                _WriteSafeFileHandle = APICalls.CreateFile(DeviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);

                if (_ReadSafeFileHandle.IsInvalid)
                {
                    throw new ApiException(Messages.ErrorMessageCantOpenRead);
                }

                IsReadOnly = _WriteSafeFileHandle.IsInvalid ? true : false;

                if (IsReadOnly.Value)
                {
                    Logger?.Log($"Opening device {DeviceId} in radonly mode.", nameof(WindowsHidDevice), null, LogLevel.Warning);
                }

                ConnectedDeviceDefinition = WindowsHidDeviceFactory.GetDeviceDefinition(DeviceId, _ReadSafeFileHandle);

                var readBufferSize = ReadBufferSize;
                var writeBufferSize = WriteBufferSize;

                if (readBufferSize == 0)
                {
                    throw new ValidationException($"{nameof(ReadBufferSize)} must be specified. HidD_GetAttributes may have failed or returned an InputReportByteLength of 0. Please specify this argument in the constructor");
                }

                _ReadFileStream = new FileStream(_ReadSafeFileHandle, FileAccess.Read, readBufferSize, false);

                if (!IsReadOnly.Value)
                {
                    if (writeBufferSize == 0)
                    {
                        throw new ValidationException($"{nameof(WriteBufferSize)} must be specified. HidD_GetAttributes may have failed or returned an OutputReportByteLength of 0. Please specify this argument in the constructor");
                    }

                    //Don't open if this is a read only connection
                    _WriteFileStream = new FileStream(_WriteSafeFileHandle, FileAccess.ReadWrite, writeBufferSize, false);
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

        public override async Task<ReadResult> ReadAsync()
        {
            var data = (await ReadReportAsync()).Data;
            Tracer?.Trace(false, data);
            return data;
        }

        public async Task<ReadReport> ReadReportAsync()
        {
            byte? reportId = null;

            if (_ReadFileStream == null)
            {
                throw new NotInitializedException(Messages.ErrorMessageNotInitialized);
            }

            var bytes = new byte[ReadBufferSize];

            try
            {
                await _ReadFileStream.ReadAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Log(Messages.ReadErrorMessage, ex);
                throw new IOException(Messages.ReadErrorMessage, ex);
            }

            if (ReadBufferHasReportId) reportId = bytes.First();

            var retVal = ReadBufferHasReportId ? RemoveFirstByte(bytes) : bytes;

            return new ReadReport(reportId, retVal);
        }

        public override Task WriteAsync(byte[] data)
        {
            return WriteReportAsync(data, 0);
        }

        public async Task WriteReportAsync(byte[] data, byte? reportId)
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
                    await _WriteFileStream.WriteAsync(bytes, 0, bytes.Length);
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