using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net
{
    public class UsbInterfaceManager : IDisposable
    {
        #region Fields
        private bool disposed;
        private IUsbInterface _ReadUsbInterface;
        private IUsbInterface _WriteUsbInterface;
        private IUsbInterface _ReadInterruptUsbInterface;
        private IUsbInterface _WriteInterruptUsbInterface;
        #endregion

        #region Protected Properties
        protected ILogger Logger { get; }
        #endregion

        #region Constructor
        public UsbInterfaceManager(ILogger logger)
        {
            Logger = logger;
        }
        #endregion

        #region Protected Methods
        public void RegisterDefaultInterfaces()
        {
            foreach (var usbInterface in UsbInterfaces)
            {
                usbInterface.RegisterDefaultEndpoints();
            }

            ReadUsbInterface = UsbInterfaces.FirstOrDefault(i => i.ReadEndpoint != null);
            WriteUsbInterface = UsbInterfaces.FirstOrDefault(i => i.WriteEndpoint != null);
            ReadInterruptUsbInterface = UsbInterfaces.FirstOrDefault(i => i.InterruptReadEndpoint != null);
            WriteInterruptUsbInterface = UsbInterfaces.FirstOrDefault(i => i.InterruptWriteEndpoint != null);

            Logger?.LogInformation("Read interface: {readInterface} Write interface {writeInterface} ReadInterruptUsbInterface: {}", ReadUsbInterface?.InterfaceNumber, WriteUsbInterface?.InterfaceNumber, ReadInterruptUsbInterface?.InterfaceNumber, WriteInterruptUsbInterface?.InterfaceNumber);
        }
        #endregion

        #region Public Properties        
        public IList<IUsbInterface> UsbInterfaces { get; } = new List<IUsbInterface>();
        public IUsbInterface ReadUsbInterface
        {
            get => _ReadUsbInterface;
            set
            {
                if (value != null && !UsbInterfaces.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidInterface);
                _ReadUsbInterface = value;
            }
        }

        public IUsbInterface WriteUsbInterface
        {
            get => _WriteUsbInterface;
            set
            {
                if (value != null && !UsbInterfaces.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidInterface);
                _WriteUsbInterface = value;
            }
        }

        public IUsbInterface ReadInterruptUsbInterface
        {
            get => _ReadInterruptUsbInterface;
            set
            {
                if (value != null && !UsbInterfaces.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidInterface);
                _ReadInterruptUsbInterface = value;
            }
        }

        public IUsbInterface WriteInterruptUsbInterface
        {
            get => _WriteInterruptUsbInterface;
            set
            {
                if (value != null && !UsbInterfaces.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidInterface);
                _WriteInterruptUsbInterface = value;
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
