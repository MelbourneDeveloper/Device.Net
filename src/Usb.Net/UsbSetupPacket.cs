namespace Usb.Net
{
    public class UsbSetupPacket
    {
        /// <summary>
        /// Creates a UsbSetupPacket object.
        /// </summary>
        public UsbSetupPacket()
        {

        }

        ///// <summary>
        ///// Creates a UsbSetupPacket object from a formatted buffer (eight bytes) that contains the setup packet.
        ///// </summary>
        ///// <param name="">A caller-supplied buffer that contains the setup packet formatted as per the standard USB specification.
        ///// The length of the buffer must be eight bytes because that is the size of a setup packet on the bus.</param>
        //public UsbSetupPacket(byte[] buffer)
        //{
        //    //TODO: add implementation
        //}

        /// <summary>
        /// Gets or sets the wIndex field in the setup packet of the USB control transfer.
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// Gets the length, in bytes, of the setup packet.
        /// </summary>
        public uint Length { get; set; }


        /// <summary>
        /// Gets or sets the bRequest field in the setup packet of the USB control transfer.
        /// </summary>
        public byte Request { get; set; }

        /// <summary>
        /// Gets or sets the bmRequestType field in the setup packet of the USB control transfer. That field is represented by a UsbControlRequestType object.
        /// </summary>
        public UsbControlRequestType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the wValue field in the setup packet of the USB control transfer.
        /// </summary>
        public uint Value { get; set; }
    }
}
