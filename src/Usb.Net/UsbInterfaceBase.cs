using Device.Net;
using Device.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net
{
    public abstract class UsbInterfaceBase
    {
        #region Fields
        private IUsbInterfaceEndpoint _BulkReadEndpoint;
        private IUsbInterfaceEndpoint _BulkWriteEndpoint;
        private IUsbInterfaceEndpoint _WriteInterruptEndpoint;
        private IUsbInterfaceEndpoint _ReadInterruptEndpoint;
        private readonly ushort? _ReadBufferSize;
        private readonly ushort? _WriteBufferSize;
        #endregion

        #region Public Properties
        public ILogger Logger { get; }
        public ITracer Tracer { get; }

        public ushort ReadBufferSize
        {
            get
            {
                if (_ReadBufferSize.HasValue) return _ReadBufferSize.Value;

                if (BulkReadEndpoint != null) return BulkReadEndpoint.ReadBufferSize;

                if (InterruptReadEndpoint != null) return InterruptReadEndpoint.ReadBufferSize;

                throw new NotImplementedException();
            }
        }

        public ushort WriteBufferSize
        {
            get
            {
                if (_WriteBufferSize.HasValue) return _WriteBufferSize.Value;

                if (BulkWriteEndpoint != null) return BulkWriteEndpoint.ReadBufferSize;

                if (InterruptWriteEndpoint != null) return InterruptWriteEndpoint.ReadBufferSize;

                throw new NotImplementedException();
            }
        }

        public IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; } = new List<IUsbInterfaceEndpoint>();

        public IUsbInterfaceEndpoint BulkReadEndpoint
        {
            get => _BulkReadEndpoint ?? (_BulkReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsRead && !p.IsInterrupt));
            set
            {
                if (value!=null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _BulkReadEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint BulkWriteEndpoint
        {
            get => _BulkWriteEndpoint ?? (_BulkWriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsWrite && !p.IsInterrupt));
            set
            {
                if (value != null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _BulkWriteEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint InterruptWriteEndpoint
        {
            get => _WriteInterruptEndpoint ?? (_WriteInterruptEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsInterrupt && p.IsWrite));
            set
            {
                if (value != null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _WriteInterruptEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint InterruptReadEndpoint
        {
            get => _ReadInterruptEndpoint ?? (_ReadInterruptEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsInterrupt && p.IsRead));
            set
            {
                if (value != null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _ReadInterruptEndpoint = value;
            }
        }
        #endregion

        #region Public Methods
        public void RegisterDefaultEndpoints()
        {
            //TODO: This should look for bulk transfer, not not interrupt
            BulkReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsRead && !e.IsInterrupt);
            BulkWriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsWrite && !e.IsInterrupt);

            InterruptReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsRead && e.IsInterrupt);
            InterruptWriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsWrite && e.IsInterrupt);
        }


        /// <summary>
        /// Note: some platforms require a call to be made to claim the interface. This is currently only for Android but may change
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS1998
        public virtual async Task ClaimInterface()
#pragma warning restore CS1998 
        {
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
