using System;

namespace Usb.Net
{
    public class ControlTransferException : Exception
    {
        public ControlTransferException(string message) : base(message)
        {
        }

        public ControlTransferException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ControlTransferException()
        {
        }
    }
}
