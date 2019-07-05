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

        #region Constructor
        protected UsbDeviceHandlerBase(ILogger logger, ITracer tracer)
        {
            Tracer = tracer;
            Logger = logger;
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
