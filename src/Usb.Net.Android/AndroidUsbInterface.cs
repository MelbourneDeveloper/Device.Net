using Android.Hardware.Usb;
using Device.Net;
using Java.Nio;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Usb.Net.Android
{
    internal class AndroidUsbInterface : UsbInterfaceBase, IUsbInterface
    {
        private readonly UsbInterface _UsbInterface;
        private readonly UsbDeviceConnection _UsbDeviceConnection;

        public AndroidUsbInterface(UsbInterface usbInterface, UsbDeviceConnection usbDeviceConnection, ILogger logger, ITracer tracer) : base(logger, tracer)
        {
            _UsbInterface = usbInterface;
            _UsbDeviceConnection = usbDeviceConnection;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> ReadAsync(uint bufferLength)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var byteBuffer = ByteBuffer.Allocate((int)bufferLength);
                    var request = new UsbRequest();
                    var endpoint = ((AndroidUsbEndpoint)ReadEndpoint).UsbEndpoint;
                    request.Initialize(_UsbDeviceConnection, endpoint);
#pragma warning disable CS0618 
                    request.Queue(byteBuffer, (int)bufferLength);
#pragma warning restore CS0618 
                    await _UsbDeviceConnection.RequestWaitAsync();
                    var buffers = new byte[bufferLength];

                    byteBuffer.Rewind();

                    //Ouch. Super nasty
                    for (var i = 0; i < bufferLength; i++)
                    {
                        buffers[i] = (byte)byteBuffer.Get();
                    }

                    //Marshal.Copy(byteBuffer.GetDirectBufferAddress(), buffers, 0, ReadBufferLength);

                    Tracer?.Trace(false, buffers);

                    return buffers;
                }
                catch (Exception ex)
                {
                    Logger?.Log(Helpers.ReadErrorMessage, nameof(AndroidUsbDeviceHandler), ex, LogLevel.Error);
                    throw new IOException(Helpers.ReadErrorMessage, ex);
                }
            });
        }

        public async Task WriteAsync(byte[] data)
        {
            await Task.Run(async () =>
            {
               try
               {
                   var request = new UsbRequest();
                   var endpoint = ((AndroidUsbEndpoint)WriteEndpoint).UsbEndpoint;
                   request.Initialize(_UsbDeviceConnection, endpoint);
                   var byteBuffer = ByteBuffer.Wrap(data);

                   Tracer?.Trace(true, data);

#pragma warning disable CS0618 
                    request.Queue(byteBuffer, data.Length);
#pragma warning restore CS0618 
                    await _UsbDeviceConnection.RequestWaitAsync();
               }
               catch (Exception ex)
               {
                   Logger?.Log(Helpers.WriteErrorMessage, nameof(AndroidUsbInterface), ex, LogLevel.Error);
                   throw new IOException(Helpers.WriteErrorMessage, ex);
               }
            });
        }
    }
}