using Device.Net;
using Device.Net.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
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
        private bool disposedValue = false;
        #endregion

        #region Public Properties
        public windowsUsbInterface UsbInterface { get; }
        public byte InterfaceNumber => UsbInterface.InterfaceNumber;
        public override string ToString() => InterfaceNumber.ToString();
        #endregion

        #region Public Methods
        public UWPUsbInterface(windowsUsbInterface usbInterface, ILogger logger, ITracer tracer) : base(logger, tracer)
        {
            UsbInterface = usbInterface ?? throw new ArgumentNullException(nameof(usbInterface));

            foreach (var inPipe in usbInterface.InterruptInPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceInterruptReadEndpoint(inPipe, Logger, Tracer);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (InterruptReadEndpoint == null) InterruptReadEndpoint = uwpUsbInterfaceEndpoint;
            }

            foreach (var outPipe in usbInterface.InterruptOutPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbInterruptOutPipe>(outPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (InterruptWriteEndpoint == null) InterruptWriteEndpoint = uwpUsbInterfaceEndpoint;
            }

            foreach (var inPipe in usbInterface.BulkInPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbBulkInPipe>(inPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (BulkReadEndpoint == null) BulkReadEndpoint = uwpUsbInterfaceEndpoint;
            }

            foreach (var outPipe in usbInterface.BulkOutPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbBulkOutPipe>(outPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (BulkWriteEndpoint == null) BulkWriteEndpoint = uwpUsbInterfaceEndpoint;
            }

            //TODO: Why does not UWP not support Control Transfer?
        }

        public async Task<byte[]> ReadAsync(uint bufferLength)
        {
            if (BulkReadEndpoint == null) throw new ValidationException(Messages.ErrorMessageNotInitialized);

            IBuffer buffer = null;

            if (BulkReadEndpoint is UWPUsbInterfaceInterruptReadEndpoint usbInterruptInPipe)
            {
                return await usbInterruptInPipe.ReadAsync();
            }
            else if (BulkWriteEndpoint is UWPUsbInterfaceEndpoint<UsbBulkInPipe> usbBulkInPipe)
            {
                buffer = new wss.Buffer(bufferLength);
                await usbBulkInPipe.Pipe.InputStream.ReadAsync(buffer, bufferLength, InputStreamOptions.None);
            }
            else
            {
                throw new DeviceException(Messages.ErrorMessageReadEndpointNotRecognized);
            }

            return buffer.ToArray();
        }

        public async Task WriteAsync(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (BulkWriteEndpoint == null) throw new ValidationException(Messages.ErrorMessageNotInitialized);

            if (data.Length > WriteBufferSize) throw new ValidationException(Messages.ErrorMessageBufferSizeTooLarge);

            var buffer = data.AsBuffer();

            uint count = 0;

            if (BulkWriteEndpoint is UWPUsbInterfaceEndpoint<UsbInterruptOutPipe> usbInterruptOutPipe)
            {
                count = await usbInterruptOutPipe.Pipe.OutputStream.WriteAsync(buffer);

            }
            else if (BulkWriteEndpoint is UWPUsbInterfaceEndpoint<UsbBulkOutPipe> usbBulkOutPipe)
            {
                count = await usbBulkOutPipe.Pipe.OutputStream.WriteAsync(buffer);
            }
            else
            {
                throw new DeviceException(Messages.ErrorMessageWriteEndpointNotRecognized);
            }

            if (count == data.Length)
            {
                Tracer?.Trace(true, data);
            }
            else
            {
                var message = Messages.GetErrorMessageInvalidWriteLength(data.Length, count);
                Logger?.Log(message, GetType().Name, null, LogLevel.Error);
                throw new IOException(message);
            }

        }
        #endregion

        #region IDisposable Support
        public void Dispose()
        {
            if (disposedValue) return;
            disposedValue = true;
        }
        #endregion
    }
}
