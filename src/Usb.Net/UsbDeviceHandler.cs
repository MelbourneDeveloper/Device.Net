using Device.Net;
using System;
using System.Collections.Generic;

namespace Usb.Net
{
    public abstract class UsbDeviceHandlerBase
    {
        #region Fields
        private IUsbInterface _ReadUsbInterface;
        private IUsbInterface _WriteUsbInterface;
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
                if (!UsbInterfaces.Contains(value)) throw new Exception("The interface is not contained the list of valid interfaces.");
                _ReadUsbInterface = value;
            }
        }

        public IUsbInterface WriteUsbInterface
        {
            get => _WriteUsbInterface;
            set
            {
                if (!UsbInterfaces.Contains(value)) throw new Exception("The interface is not contained the list of valid interfaces.");
                _WriteUsbInterface = value;
            }
        }
        #endregion
    }
}
