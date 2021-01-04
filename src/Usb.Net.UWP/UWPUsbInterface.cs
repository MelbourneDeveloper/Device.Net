using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Usb;
using Windows.Storage.Streams;
using windowsUsbInterface = Windows.Devices.Usb.UsbInterface;
using wss = Windows.Storage.Streams;

namespace Usb.Net.UWP
{
    public class UWPUsbInterface : UsbInterfaceBase, IUsbInterface
    {
        #region Fields
        private bool disposed;
        #endregion

        #region Public Properties
        public windowsUsbInterface UsbInterface { get; }
        public override byte InterfaceNumber => UsbInterface.InterfaceNumber;
        public override string ToString() => InterfaceNumber.ToString();
        #endregion

        #region Constructor
        public UWPUsbInterface(
            windowsUsbInterface usbInterface,
            PerformControlTransferAsync performControlTransferAsync,
            IDataReceiver dataReceiver,
            ILogger logger = null,
            ushort? readBuffersize = null,
            ushort? writeBufferSize = null) : base(
                performControlTransferAsync,
                logger,
                readBuffersize,
                writeBufferSize)
        {
            UsbInterface = usbInterface ?? throw new ArgumentNullException(nameof(usbInterface));

            foreach (var inPipe in usbInterface.InterruptInPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceInterruptReadEndpoint(
                    inPipe,
                    dataReceiver,
                    Logger);

                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                InterruptReadEndpoint ??= uwpUsbInterfaceEndpoint;
            }

            foreach (var outPipe in usbInterface.InterruptOutPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbInterruptOutPipe>(outPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                InterruptWriteEndpoint ??= uwpUsbInterfaceEndpoint;
            }

            foreach (var inPipe in usbInterface.BulkInPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbBulkInPipe>(inPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                ReadEndpoint ??= uwpUsbInterfaceEndpoint;
            }

            foreach (var outPipe in usbInterface.BulkOutPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbBulkOutPipe>(outPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                WriteEndpoint ??= uwpUsbInterfaceEndpoint;
            }

            //TODO: Why does not UWP not support Control Transfer?
        }
        #endregion

        #region Public Methods
        public async Task<TransferResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default)
        {
            IBuffer buffer;

            if (ReadEndpoint is UWPUsbInterfaceEndpoint<UsbBulkInPipe> usbBulkInPipe)
            {
                buffer = new wss.Buffer(bufferLength);
                _ = await usbBulkInPipe.Pipe.InputStream.ReadAsync(buffer, bufferLength, InputStreamOptions.None).AsTask(cancellationToken);
                //TODO: Seems there is no way to figure out how much data was read?
                return new TransferResult(buffer.ToArray(), buffer.Length);
            }
            else
            {
                return InterruptReadEndpoint is UWPUsbInterfaceInterruptReadEndpoint usbInterruptInPipe
                    ? await usbInterruptInPipe.ReadAsync(cancellationToken)
                    : throw new DeviceException(Messages.ErrorMessageReadEndpointNotRecognized);
            }

        }

        public async Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            //TODO: It might not be the case that Initialize has not been called. Better error message here please.
            if (WriteEndpoint == null && InterruptWriteEndpoint == null) throw new ValidationException(Messages.ErrorMessageNotInitialized);

            if (data.Length > WriteBufferSize) throw new ValidationException(Messages.ErrorMessageBufferSizeTooLarge);

            using var logScope = Logger?.BeginScope("Interface number: {interfaceNumber} Call: {call}", UsbInterface.InterfaceNumber, nameof(WriteAsync));

            try
            {
                var buffer = data.AsBuffer();

                uint count = 0;

                if (WriteEndpoint is UWPUsbInterfaceEndpoint<UsbBulkOutPipe> usbBulkOutPipe)
                {
                    count = await usbBulkOutPipe.Pipe.OutputStream.WriteAsync(buffer).AsTask(cancellationToken);
                }
                else if (InterruptWriteEndpoint is UWPUsbInterfaceEndpoint<UsbInterruptOutPipe> usbInterruptOutPipe)
                {
                    //Falling back to interrupt

                    Logger?.LogWarning(Messages.WarningMessageWritingToInterrupt);
                    count = await usbInterruptOutPipe.Pipe.OutputStream.WriteAsync(buffer);
                }

                else
                {
                    throw new DeviceException(Messages.ErrorMessageWriteEndpointNotRecognized);
                }

                if (count == data.Length)
                {
                    Logger.LogDataTransfer(new Trace(true, data));
                }
                else
                {
                    throw new IOException(Messages.GetErrorMessageInvalidWriteLength(data.Length, count));
                }

                return count;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.WarningMessageWritingToInterrupt);
                throw;
            }
        }

        #endregion

        #region IDisposable Support
        public void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, UsbInterface?.ToString());
                return;
            }

            disposed = true;

            Logger.LogInformation("Dispose called on the interface but not really doing anything...", UsbInterface?.ToString());
        }
        #endregion
    }
}
