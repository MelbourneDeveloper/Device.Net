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
        private IUsbInterfaceEndpoint _ReadEndpoint;
        private IUsbInterfaceEndpoint _WriteEndpoint;
        private IUsbInterfaceEndpoint _WriteInterruptEndpoint;
        private IUsbInterfaceEndpoint _ReadInterruptEndpoint;
        private readonly ushort? _ReadBufferSize;
        private readonly ushort? _WriteBufferSize;
        public abstract byte InterfaceNumber { get; }
        #endregion

        #region Public Properties
        public ILogger Logger { get; }
        public ITracer Tracer { get; }

        public ushort ReadBufferSize
        {
            get
            {
                if (_ReadBufferSize.HasValue) return _ReadBufferSize.Value;

                if (ReadEndpoint != null) return ReadEndpoint.MaxPacketSize;

                throw new NotImplementedException();
            }
        }

        public ushort WriteBufferSize
        {
            get
            {
                if (_WriteBufferSize.HasValue) return _WriteBufferSize.Value;

                if (WriteEndpoint != null) return WriteEndpoint.MaxPacketSize;

                throw new NotImplementedException();
            }
        }

        public IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; } = new List<IUsbInterfaceEndpoint>();

        public IUsbInterfaceEndpoint ReadEndpoint
        {
            get => _ReadEndpoint ?? (_ReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsRead && !p.IsInterrupt));
            set
            {
                if (value!=null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _ReadEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint WriteEndpoint
        {
            get => _WriteEndpoint ?? (_WriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsWrite && !p.IsInterrupt));
            set
            {
                if (value != null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _WriteEndpoint = value;
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
            //TODO: This should look for bulk transfer, not just not interrupt
            ReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsRead && !e.IsInterrupt);
            WriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsWrite && !e.IsInterrupt);

            InterruptReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsRead && e.IsInterrupt);
            InterruptWriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(e => e.IsWrite && e.IsInterrupt);

            //This falls back on the interrupt endpoint if there is not bulk pipes. This is the just the oddbal scenario
            if (ReadEndpoint == null && InterruptReadEndpoint != null)
            {
                ReadEndpoint = InterruptReadEndpoint;
                Logger.Log(Messages.GetErrorMessageNoBulkPipe(InterfaceNumber, true), nameof(UsbInterfaceBase), null, LogLevel.Warning);
            }

            if (WriteEndpoint == null && InterruptWriteEndpoint != null)
            {
                WriteEndpoint = InterruptWriteEndpoint;
                Logger.Log(Messages.GetErrorMessageNoBulkPipe(InterfaceNumber, false), nameof(UsbInterfaceBase), null, LogLevel.Warning);
            }
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
