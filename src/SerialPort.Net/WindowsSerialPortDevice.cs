using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SerialPort.Net.Windows
{
    public class WindowsSerialPortDevice : DeviceBase, IDevice, IDisposable
    {
        #region Fields
        private readonly int _BaudRate;
        private readonly byte _ByteSize;
        private bool disposed;
        private readonly Parity _Parity;
        private SafeFileHandle _ReadSafeFileHandle;
        private readonly StopBits _StopBits;
        private readonly ushort _ReadBufferSize;
        #endregion

        #region Public Properties
        public override bool IsInitialized => _ReadSafeFileHandle != null && !_ReadSafeFileHandle.IsInvalid;
        /// <summary>
        /// TODO: No need to implement this. The property probably shouldn't exist at the base level
        /// </summary>
        public override ushort WriteBufferSize => 0;
        public override ushort ReadBufferSize => _ReadBufferSize;
        public IApiService ApiService { get; }
        #endregion

        #region Constructor
        public WindowsSerialPortDevice(string deviceId) : this(deviceId, new ApiService(null), 9600, StopBits.One, Parity.None, 8, 1024, null, null)
        {
        }

        public WindowsSerialPortDevice(string deviceId, IApiService apiService, int baudRate, StopBits stopBits, Parity parity, byte byteSize, ushort readBufferSize, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
            ApiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            ConnectedDeviceDefinition = new ConnectedDeviceDefinition(DeviceId);

            if ((byteSize == 5 && stopBits == StopBits.Two) || (stopBits == StopBits.OnePointFive && byteSize > 5))
                throw new ArgumentException(Messages.ErrorInvalidByteSizeAndStopBitsCombo);

            if (byteSize < 5 || byteSize > 8)
                throw new ArgumentOutOfRangeException(nameof(byteSize), Messages.ErrorByteSizeMustBeFiveToEight);

            if (baudRate < 110 || baudRate > 256000)
                throw new ArgumentOutOfRangeException(nameof(baudRate), Messages.ErrorBaudRateInvalid);

            if (stopBits == StopBits.None)
                throw new ArgumentException(Messages.ErrorMessageStopBitsMustBeSpecified, nameof(stopBits));

            _ReadBufferSize = readBufferSize;
            _BaudRate = baudRate;
            _ByteSize = byteSize;
            _StopBits = stopBits;
            _Parity = parity;
        }
        #endregion

        #region Public Methods
        public Task InitializeAsync()
        {
            return Task.Run(() => { Initialize(); });
        }

        private int Write(byte[] data)
        {
            return data == null ? 0 : ApiService.AWriteFile(_ReadSafeFileHandle, data, data.Length, out var bytesWritten, 0) ? bytesWritten : -1;
        }

        public override Task WriteAsync(byte[] data)
        {
            ValidateConnection();
            return Task.Run(() => { Write(data); });
        }

        public override Task<ReadResult> ReadAsync()
        {
            ValidateConnection();

            return Task.Run(() =>
            {
                var buffer = new byte[_ReadBufferSize];
                var bytesRead = Read(buffer);
                return new ReadResult(buffer, bytesRead);
            });
        }

        public override Task Flush()
        {
            ValidateConnection();

            return Task.Run(() => ApiService.APurgeComm(_ReadSafeFileHandle, APICalls.PURGE_RXCLEAR | APICalls.PURGE_TXCLEAR));
        }

        public override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            if (_ReadSafeFileHandle != null)
            {
                _ReadSafeFileHandle.Dispose();
                _ReadSafeFileHandle = new SafeFileHandle((IntPtr)0, true);
            }

            base.Dispose();
        }

        public void Close()
        {
            Dispose();
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            _ReadSafeFileHandle = ApiService.CreateReadConnection(DeviceId, FileAccessRights.GenericRead | FileAccessRights.GenericWrite);

            if (_ReadSafeFileHandle.IsInvalid) return;

            var dcb = new Dcb();

            var isSuccess = ApiService.AGetCommState(_ReadSafeFileHandle, ref dcb);

            WindowsDeviceBase.HandleError(isSuccess, Messages.ErrorCouldNotGetCommState);

            dcb.ByteSize = _ByteSize;
            dcb.fDtrControl = 1;
            dcb.BaudRate = (uint)_BaudRate;
            dcb.fBinary = 1;
            dcb.fTXContinueOnXoff = 0;
            dcb.fAbortOnError = 0;

            dcb.fParity = 1;
            switch (_Parity)
            {
                case Parity.Even:
                    dcb.Parity = 2;
                    break;
                case Parity.Mark:
                    dcb.Parity = 3;
                    break;
                case Parity.Odd:
                    dcb.Parity = 1;
                    break;
                case Parity.Space:
                    dcb.Parity = 4;
                    break;
                default:
                    dcb.Parity = 0;
                    break;
            }

            switch (_StopBits)
            {
                case StopBits.One:
                    dcb.StopBits = 0;
                    break;
                case StopBits.OnePointFive:
                    dcb.StopBits = 1;
                    break;
                case StopBits.Two:
                    dcb.StopBits = 2;
                    break;
                default:
                    throw new ArgumentException(Messages.ErrorMessageStopBitsMustBeSpecified);
            }

            isSuccess = ApiService.ASetCommState(_ReadSafeFileHandle, ref dcb);
            WindowsDeviceBase.HandleError(isSuccess, Messages.ErrorCouldNotSetCommState);

            var timeouts = new CommTimeouts
            {
                WriteTotalTimeoutConstant = 0,
                ReadIntervalTimeout = 1,
                WriteTotalTimeoutMultiplier = 0,
                ReadTotalTimeoutMultiplier = 0,
                ReadTotalTimeoutConstant = 0
            };

            isSuccess = ApiService.ASetCommTimeouts(_ReadSafeFileHandle, ref timeouts);
            WindowsDeviceBase.HandleError(isSuccess, Messages.ErrorCouldNotSetCommTimeout);
        }

        private uint Read(byte[] data)
        {
            if (ApiService.AReadFile(_ReadSafeFileHandle, data, data.Length, out var bytesRead, 0)) return bytesRead;

            throw new IOException(Messages.ErrorMessageRead);
        }

        private void ValidateConnection()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(Messages.ErrorMessageNotInitialized);
            }
        }
        #endregion
    }
}