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
        private IUsbInterfaceEndpoint _WriteInterruptEndpoint;
        private IUsbInterfaceEndpoint _ReadInterruptEndpoint;
        #endregion

        #region Public Properties
        public ILogger Logger { get;  }
        public ITracer Tracer { get; }
        public ushort ReadBufferSize => _ReadEndpoint.ReadBufferSize;
        public ushort WriteBufferSize => _WriteEndpoint.WriteBufferSize;

        public IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; } = new List<IUsbInterfaceEndpoint>();

        public IUsbInterfaceEndpoint ReadEndpoint
        {
            get => _ReadEndpoint ?? (_ReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsRead && !p.IsInterrupt));
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _ReadEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint WriteEndpoint
        {
            get => _WriteEndpoint ?? (_WriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsWrite && !p.IsInterrupt));
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _WriteEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint WriteInterruptEndpoint
        {
            get => _WriteInterruptEndpoint ?? (_WriteInterruptEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsInterrupt && p.IsWrite));
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _WriteInterruptEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint ReadInterruptEndpoint
        {
            get => _ReadInterruptEndpoint ?? (_ReadInterruptEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsInterrupt && p.IsRead));
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _ReadInterruptEndpoint = value;
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
