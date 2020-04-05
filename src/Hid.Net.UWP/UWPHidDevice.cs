using Device.Net;
using Device.Net.Exceptions;
using Device.Net.UWP;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Storage;

namespace Hid.Net.UWP
{
    //TODO: Make this class inherit from DeviceBase

    public class UWPHidDevice : UWPDeviceBase<HidDevice>, IHidDevice
    {
        #region Fields
        private bool disposed;
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        #endregion

        #region Public Properties
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
        private void HidDevice_InputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
        {
            HandleDataReceived(InputReportToBytes(args));
        }
        #endregion

        #region Constructors
        public UWPHidDevice(ILogger logger, ITracer tracer) : this(null, logger, tracer)
        {
        }

        public UWPHidDevice(string deviceId) : this(deviceId, null, null)
        {
        }

        public UWPHidDevice(string deviceId, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
        {
        }
        #endregion

        #region Private Methods
        public override async Task InitializeAsync()
        {
            //TODO: Put a lock here to stop reentrancy of multiple calls

            if (disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

            Log("Initializing Hid device", null);

            await GetDeviceAsync(DeviceId);

            if (ConnectedDevice != null)
            {
                ConnectedDevice.InputReportReceived += HidDevice_InputReportReceived;
            }
            else
            {
                throw new DeviceException($"The device {DeviceId} failed to initialize");
            }
        }

        protected override IAsyncOperation<HidDevice> FromIdAsync(string id)
        {
            return GetHidDevice(id);
        }
        #endregion

        #region Private Static Methods
        private static byte[] InputReportToBytes(HidInputReportReceivedEventArgs args)
        {
            byte[] bytes;
            using (var stream = args.Report.Data.AsStream())
            {
                bytes = new byte[args.Report.Data.Length];
                stream.Read(bytes, 0, (int)args.Report.Data.Length);
            }

            return bytes;
        }
        #endregion

        #region Public Methods
        public override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            _WriteAndReadLock.Dispose();

            base.Dispose();
        }

        public virtual Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            return WriteReportAsync(data, 0, cancellationToken);
        }

        public async Task WriteReportAsync(byte[] data, byte? reportId, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            byte[] bytes;
            if (reportId.HasValue || DefaultReportId.HasValue)
            {
                bytes = new byte[data.Length + 1];
                Array.Copy(data, 0, bytes, 1, data.Length);
                bytes[0] = reportId ?? DefaultReportId.Value;
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
                    Tracer?.Trace(true, bytes);
                }
                else
                {
                    var message = Messages.GetErrorMessageInvalidWriteLength(bytes.Length, count);
                    Logger?.Log(message, GetType().Name, null, LogLevel.Error);
                    throw new IOException(message);
                }
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
            var bytes = await base.ReadAsync(cancellationToken);

            if (DefaultReportId.HasValue)
            {
                reportId = bytes.Data[0];
                bytes = DeviceBase.RemoveFirstByte(bytes);
            }

            return new ReadReport(reportId, bytes);
        }

        public override async Task<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            var data = (await ReadReportAsync(cancellationToken)).Data;
            Tracer?.Trace(false, data);
            return data;
        }
        #endregion

        #region Public Static Methods
        public static IAsyncOperation<HidDevice> GetHidDevice(string id)
        {
            return HidDevice.FromIdAsync(id, FileAccessMode.ReadWrite);
        }

        public async Task<ReadResult> WriteAndReadAsync(byte[] writeBuffer, CancellationToken cancellationToken = default)
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                await WriteAsync(writeBuffer, cancellationToken);
                var retVal = await ReadAsync(cancellationToken);
                Logger?.Log(Messages.SuccessMessageWriteAndReadCalled, nameof(UWPHidDevice), null, LogLevel.Information);
                return retVal;
            }
            catch (Exception ex)
            {
                Log(Messages.ErrorMessageReadWrite, ex);
                throw;
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        public Task Flush(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(Messages.ErrorMessageFlushNotImplemented);
        }
        #endregion
    }
}
