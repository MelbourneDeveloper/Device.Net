namespace Usb.Net
{
    public class UsbControlRequestType
    {
        /// <summary>
        /// Gets or sets the bmRequestType value as a byte.
        /// </summary>
        public byte AsByte { get; set; }

        /// <summary>
        /// Gets or sets the type of USB control transfer: standard, class, or vendor.
        /// </summary>
        public UsbControlTransferType ControlTransferType { get; set; }

        /// <summary>
        /// Gets or sets the direction of the USB control transfer.
        /// </summary>
        public UsbTransferDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the direction of the USB control transfer.
        /// </summary>
        public UsbControlRecipient Recipient { get; set; }

    }
}
