using Device.Net;
using Device.Net.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

            //TODO: This is totally wrong. We are only picking up Interrupt pipes here. Pipes are being used all wrong on UWP right now.

            foreach (var inPipe in usbInterface.InterruptInPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint(null, inPipe);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (WriteEndpoint == null) WriteEndpoint = uwpUsbInterfaceEndpoint;
            }

            foreach (var outPipe in usbInterface.InterruptOutPipes)
            {
                var uwpUsbInterfaceEndpoint = new UWPUsbInterfaceEndpoint(outPipe, null);
                UsbInterfaceEndpoints.Add(uwpUsbInterfaceEndpoint);
                if (ReadEndpoint == null) ReadEndpoint = uwpUsbInterfaceEndpoint;
            }

            interruptPipe.DataReceived += InterruptPipe_DataReceived;

            //TODO: Fill in the DeviceDefinition...



            _DefaultOutPipe = _DefaultConfigurationInterface.InterruptOutPipes.FirstOrDefault();

            if (_DefaultOutPipe == null) throw new DeviceException("Could not get the default out pipe for the default USB interface");

            _DefaultInPipe = _DefaultConfigurationInterface.InterruptInPipes.FirstOrDefault();

            if (_DefaultOutPipe == null) throw new DeviceException("Could not get the default in pipe for the default USB interface");

        }

        public Task<byte[]> ReadAsync(uint bufferLength)
        {
            throw new NotImplementedException();
        }

        public async Task WriteAsync(byte[] data)
        {
            if (ReadEndpoint == null) throw new ValidationException(Messages.ErrorMessageNotInitialized);

            if (data.Length > WriteBufferSize) throw new ValidationException("The buffer size is too large");
            var endpoint = (UWPUsbInterfaceEndpoint)ReadEndpoint;
            var count = await WriteEndpoint.WriteAsync(data.AsBuffer());

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
