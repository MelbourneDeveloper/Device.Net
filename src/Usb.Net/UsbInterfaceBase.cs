using Device.Net;
using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    /// <summary>
    /// Represents a USB interface
    /// </summary>
    public abstract class UsbInterfaceBase
    {
        #region Fields
        private readonly PerformControlTransferAsync _performControlTransferAsync;
        private IUsbInterfaceEndpoint _ReadEndpoint;
        private IUsbInterfaceEndpoint _WriteEndpoint;
        private IUsbInterfaceEndpoint _WriteInterruptEndpoint;
        private IUsbInterfaceEndpoint _ReadInterruptEndpoint;
        private readonly ushort? _ReadBufferSize;
        private readonly ushort? _WriteBufferSize;
        public abstract byte InterfaceNumber { get; }
        #endregion

        #region Protected Properties
        protected ILogger Logger { get; }
        #endregion

        #region Public Properties
        public ushort ReadBufferSize => _ReadBufferSize ?? ReadEndpoint?.MaxPacketSize ?? throw new NotImplementedException();

        public ushort WriteBufferSize => _WriteBufferSize ?? WriteEndpoint?.MaxPacketSize ?? throw new NotImplementedException();

        public IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; } = new List<IUsbInterfaceEndpoint>();

        public IUsbInterfaceEndpoint ReadEndpoint
        {
            get => _ReadEndpoint ??= UsbInterfaceEndpoints.FirstOrDefault(p => p.IsRead && !p.IsInterrupt);
            set
            {
                if (value != null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _ReadEndpoint = value;

#pragma warning disable CA1062 // Validate arguments of public methods
                Logger.LogInformation("ReadEndpoint set to pipeid {pipeid}", value?.PipeId);
#pragma warning restore CA1062 // Validate arguments of public methods

            }
        }

        public IUsbInterfaceEndpoint WriteEndpoint
        {
            get => _WriteEndpoint ??= UsbInterfaceEndpoints.FirstOrDefault(p => p.IsWrite && !p.IsInterrupt);
            set
            {
                if (value != null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _WriteEndpoint = value;
#pragma warning disable CA1062 // Validate arguments of public methods
                Logger.LogInformation("WriteEndpoint set to pipeid {pipeid}", value?.PipeId);
#pragma warning restore CA1062 // Validate arguments of public methods
            }
        }

        public IUsbInterfaceEndpoint InterruptWriteEndpoint
        {
            get => _WriteInterruptEndpoint ??= UsbInterfaceEndpoints.FirstOrDefault(p => p.IsInterrupt && p.IsWrite);
            set
            {
                if (value != null && !UsbInterfaceEndpoints.Contains(value)) throw new ValidationException(Messages.ErrorMessageInvalidEndpoint);
                _WriteInterruptEndpoint = value;
            }
        }

        public IUsbInterfaceEndpoint InterruptReadEndpoint
        {
            get => _ReadInterruptEndpoint ??= UsbInterfaceEndpoints.FirstOrDefault(p => p.IsInterrupt && p.IsRead);
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
                Logger.LogWarning(Messages.GetErrorMessageNoBulkPipe(InterfaceNumber, true) + " Interface # : {interfaceNumber} IsRead: {isRead} Region: {region}", InterfaceNumber, true, nameof(UsbInterfaceBase));
            }

            if (WriteEndpoint != null || InterruptWriteEndpoint == null) return;

            WriteEndpoint = InterruptWriteEndpoint;
            Logger.LogWarning(Messages.GetErrorMessageNoBulkPipe(InterfaceNumber, false) + " Interface # : {interfaceNumber} IsRead: {isRead} Region: {region}", InterfaceNumber, false, nameof(UsbInterfaceBase));
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

        public async Task<TransferResult> PerformControlTransferAsync(SetupPacket setupPacket, byte[] buffer, CancellationToken cancellationToken = default)
        {
            if (setupPacket == null) throw new ArgumentNullException(nameof(setupPacket));

            using var scope = Logger.BeginScope("Perfoming Control Transfer {setupPacket}", setupPacket);

            try
            {

                var transferBuffer = new byte[setupPacket.Length];

                if (setupPacket.Length > 0)
                {
                    if (setupPacket.RequestType.Direction == RequestDirection.Out)
                    {
                        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

                        //Make a copy so we don't mess with the array passed in
                        Array.Copy(buffer, transferBuffer, buffer.Length);
                    }
                }

                var transferResult = await _performControlTransferAsync(setupPacket, transferBuffer, cancellationToken).ConfigureAwait(false);

                if (setupPacket.RequestType.Direction == RequestDirection.Out)
                {
                    //Trace the write to the device
                    Logger.LogTrace(new Trace(true, transferBuffer));
                }

                Logger.LogInformation("Control Transfer complete {setupPacket}", setupPacket);

                var returnValue = transferResult.BytesTransferred != setupPacket.Length && setupPacket.RequestType.Direction == RequestDirection.In
                    ? throw new ControlTransferException($"Requested {setupPacket.Length} bytes but received {transferResult.BytesTransferred }")
                    : transferResult;

                if (setupPacket.RequestType.Direction == RequestDirection.In)
                {
                    //Trace the read from the device
                    Logger.LogTrace(new Trace(false, transferBuffer));
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error on {nameof(PerformControlTransferAsync)}");
                throw;
            }
        }
        #endregion

        #region Constructor
        protected UsbInterfaceBase(
            PerformControlTransferAsync performControlTransferAsync,
            ILogger logger = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null)
        {
            Logger = logger ?? NullLogger.Instance;
            _ReadBufferSize = readBufferSize;
            _WriteBufferSize = writeBufferSize;
            _performControlTransferAsync = performControlTransferAsync;
        }
        #endregion
    }
}
