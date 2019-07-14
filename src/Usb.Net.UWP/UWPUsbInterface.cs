using Device.Net;
using Device.Net.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Usb;
using wss=Windows.Storage.Streams;
using windowsUsbInterface = Windows.Devices.Usb.UsbInterface;

namespace Usb.Net.UWP
{
    public class UWPUsbInterface : UsbInterfaceBase, IUsbInterface
    {
        #region Fields
        private bool disposedValue = false;
        #endregion

        #region Public Properties
        public windowsUsbInterface UsbInterface { get; }
        #endregion

        public UWPUsbInterface(windowsUsbInterface usbInterface, ILogger logger, ITracer tracer) : base(logger, tracer)
        {
            UsbInterface = usbInterface ?? throw new ArgumentNullException(nameof(usbInterface));

            if (usbInterface.BulkInPipes.Count == 0) throw new DeviceException(Messages.ErrorMessageNoReadInterfaceFound);

            foreach (var inPipe in usbInterface.InterruptInPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbInterruptInPipe>(inPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (InterruptEndpoint == null) InterruptEndpoint = uwpUsbInterfaceEndpoint;
            }

            foreach (var inPipe in usbInterface.BulkInPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbBulkInPipe>(inPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
            }

            foreach (var outPipe in usbInterface.InterruptOutPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbInterruptOutPipe>(outPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (ReadEndpoint == null) ReadEndpoint = uwpUsbInterfaceEndpoint;
            }

            foreach (var outPipe in usbInterface.BulkOutPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint<UsbBulkOutPipe>(outPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (WriteEndpoint == null) WriteEndpoint = uwpUsbInterfaceEndpoint;
            }

            //TODO: Why does not UWP not support Control Transfer?
        }

        public async Task<byte[]> ReadAsync(uint bufferLength)
        {
            if (ReadEndpoint == null) throw new ValidationException(Messages.ErrorMessageNotInitialized);

            var endpoint = (UWPUsbInterfaceEndpoint<UsbBulkInPipe>)ReadEndpoint;

            var buffer = new wss.Buffer(bufferLength);
            await endpoint.Pipe.InputStream.ReadAsync(buffer, bufferLength, wss.InputStreamOptions.None);

            return buffer.ToArray();
        }

        public async Task WriteAsync(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (WriteEndpoint == null) throw new ValidationException(Messages.ErrorMessageNotInitialized);

            if (data.Length > WriteBufferSize) throw new ValidationException(Messages.ErrorMessageBufferSizeTooLarge);
            var endpoint = (UWPUsbInterfaceEndpoint<UsbBulkOutPipe>)WriteEndpoint;
            var count = await endpoint.Pipe.OutputStream.WriteAsync(data.AsBuffer());

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

        #region IDisposable Support
        public void Dispose()
        {
            if (disposedValue) return;
            disposedValue = true;
        }
        #endregion

    }
}
