using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net
{
    public abstract class UsbInterfaceBase 
    {
        #region Fields
        private IUsbInterfaceEndpoint _ReadEndpoint;
        private IUsbInterfaceEndpoint _WriteEndpoint;
        private readonly ushort? _ReadBufferSize;
        private readonly ushort? _WriteBufferSize;
        #endregion

        #region Public Properties
        public ILogger Logger { get;  }
        public ITracer Tracer { get; }
        public ushort ReadBufferSize => _ReadEndpoint.ReadBufferSize;
        public ushort WriteBufferSize => _WriteEndpoint.WriteBufferSize;

        public IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; } = new List<IUsbInterfaceEndpoint>();

        public IUsbInterfaceEndpoint ReadEndpoint
        {
            get
            {
                //This is a bit stinky but should work
                if (_ReadEndpoint == null)
                {
                    _ReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsRead);
                }

                return _ReadEndpoint;
            }
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new Exception("This endpoint is not contained in the list of valid endpoints");
                _ReadEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint WriteEndpoint
        {
            get
            {
                //This is a bit stinky but should work
                if (_WriteEndpoint == null)
                {
                    _WriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsWrite);
                }

                return _WriteEndpoint;
            }
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new Exception("This endpoint is not contained in the list of valid endpoints");
                _WriteEndpoint = value;
            }
        }
        #endregion

        #region Constructor
        protected UsbInterfaceBase(ILogger logger, ITracer tracer, ushort? readBufferSize, ushort? writeBufferSize)
        {
            Tracer = tracer;
            Logger = logger;
            _ReadBufferSize = readBufferSize;
            _WriteBufferSize = writeBufferSize;
        }
        #endregion
    }
}
