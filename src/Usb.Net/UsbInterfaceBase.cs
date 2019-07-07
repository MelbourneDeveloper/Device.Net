using Device.Net;
using Device.Net.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net
{
    public abstract class UsbInterfaceBase 
    {
        #region Fields
        private IUsbInterfaceEndpoint _ReadEndpoint;
        private IUsbInterfaceEndpoint _WriteEndpoint;
        private IUsbInterfaceEndpoint _InterruptEndpoint;
        #endregion

        #region Public Properties
        public ILogger Logger { get;  }
        public ITracer Tracer { get; }
        public ushort ReadBufferSize => _ReadEndpoint.ReadBufferSize;
        public ushort WriteBufferSize => _WriteEndpoint.WriteBufferSize;

        public IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; } = new List<IUsbInterfaceEndpoint>();

        public IUsbInterfaceEndpoint ReadEndpoint
        {
            get => _ReadEndpoint ?? (_ReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsRead));
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new ValidationException("This endpoint is not contained in the list of valid endpoints");
                _ReadEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint WriteEndpoint
        {
            get => _WriteEndpoint ?? (_WriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsWrite));
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new ValidationException("This endpoint is not contained in the list of valid endpoints");
                _WriteEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint InterruptEndpoint
        {
            get
            {
                //This is a bit stinky but should work
                if (_InterruptEndpoint == null)
                {
                    _InterruptEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsInterrupt);
                }

                return _InterruptEndpoint;
            }
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new ValidationException("This endpoint is not contained in the list of valid endpoints");
                _InterruptEndpoint = value;
            }
        }
        #endregion

        #region Constructor
        protected UsbInterfaceBase(ILogger logger, ITracer tracer)
        {
            Tracer = tracer;
            Logger = logger;
        }
        #endregion
    }
}
