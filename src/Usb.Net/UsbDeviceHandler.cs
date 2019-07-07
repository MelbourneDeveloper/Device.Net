using Device.Net;
using Device.Net.Exceptions;
using System;
using System.Collections.Generic;

namespace Usb.Net
{
    public abstract class UsbDeviceHandlerBase : IDisposable
    {
        #region Fields
        private bool disposed;
        private IUsbInterface _ReadUsbInterface;
        private IUsbInterface _WriteUsbInterface;
        private IUsbInterface _InterruptUsbInterface;
        #endregion

        protected ushort? _ReadBufferSize { get;  set; }
        protected ushort? _WriteBufferSize { get;  set; }

        #region Constructor
        protected UsbDeviceHandlerBase(ILogger logger, ITracer tracer, ushort? readBufferLength, ushort? writeBufferLength)
        {
            Tracer = tracer;
            Logger = logger;
            _ReadBufferSize = readBufferLength;
            _WriteBufferSize = writeBufferLength;
        }
        #endregion

        #region Public Properties        
        public ITracer Tracer { get; }
        public ILogger Logger { get; }
        public IList<IUsbInterface> UsbInterfaces { get; } = new List<IUsbInterface>();

        public IUsbInterface ReadUsbInterface
        {
            get => _ReadUsbInterface;
            set
            {
                if (!UsbInterfaces.Contains(value)) throw new ValidationException("The interface is not contained the list of valid interfaces.");
                _ReadUsbInterface = value;
            }
        }

        public IUsbInterface WriteUsbInterface
        {
            get => _WriteUsbInterface;
            set
            {
                if (!UsbInterfaces.Contains(value)) throw new ValidationException("The interface is not contained the list of valid interfaces.");
                _WriteUsbInterface = value;
            }
        }

        public IUsbInterface InterruptUsbInterface
        {
            get => _InterruptUsbInterface;
            set
            {
                if (!UsbInterfaces.Contains(value)) throw new ValidationException("The interface is not contained the list of valid interfaces.");
                _InterruptUsbInterface = value;
            }
        }
        public virtual void Dispose()
        {
            if (disposed) return;
            disposed = true;

            foreach (var usbInterface in UsbInterfaces)
            {
                usbInterface.Dispose();
            }

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
