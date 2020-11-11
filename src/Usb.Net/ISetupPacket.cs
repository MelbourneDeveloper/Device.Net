namespace Usb.Net
{
    public interface ISetupPacket
    {
        /// <summary>
        /// Gets or sets the wIndex field in the setup packet of the USB control transfer.
        /// </summary>
        uint Index { get; set; }

        /// <summary>
        /// Gets the length, in bytes, of the setup packet.
        /// </summary>
        uint Length { get; set; }


        /// <summary>
        /// Gets or sets the bRequest field in the setup packet of the USB control transfer.
        /// </summary>
        byte Request { get; set; }

        /// <summary>
        /// Gets or sets the bmRequestType field in the setup packet of the USB control transfer. That field is represented by a UsbControlRequestType object.
        /// </summary>
        UsbControlRequestType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the wValue field in the setup packet of the USB control transfer.
        /// </summary>
        uint Value { get; set; }
    }
}
