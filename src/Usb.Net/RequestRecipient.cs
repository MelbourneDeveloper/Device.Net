// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Usb.Net
{
    /// <summary>
    /// Defines constants that indicate the recipient of a USB control transfer. 
    /// The recipient is defined in the setup packet of the control request.
    /// </summary>
    /// <remarks>
    /// See Table 9.2 of section 9.3 of the Universal Serial Bus (USB) specification (<see href="http://www.usb.org"/>)
    /// </remarks>
    public enum RequestRecipient
    {
        ///// <summary>
        ///// The recipient of the control transfer is the default (or the first) USB interface in the selected configuration of the device.
        ///// If the recipient is the first interface of the active configuration(DefaultInterface ), 
        ///// SendControlInTransferAsync and SendControlOutTransferAsync methods overwrite the low byte of UsbSetupPacket.Index with the interface number of the default interface.
        ///// 
        ///// By using this value, an app can omit the interface number in an interface-recipient request.
        ///// </summary>
        //DefaultInterface = 4,

        /// <summary>
        /// The recipient of the control transfer is the device.
        /// </summary>
        Device = 0,

        /// <summary>
        /// The recipient of the control transfer is the USB interface that is specified in the request.
        /// </summary>
        Interface = 1,

        /// <summary>
        /// The recipient of the control transfer is an endpoint.
        /// </summary>
        Endpoint = 2,

        /// <summary>
        /// The recipient of the control transfer is other.
        /// </summary>
        Other = 3,
    }
}
