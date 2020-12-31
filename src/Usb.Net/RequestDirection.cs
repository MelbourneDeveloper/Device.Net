namespace Usb.Net
{
    /// <summary>
    /// Defines constants that indicate the direction of USB transfer: IN or OUT transfers.
    /// </summary>
    /// <remarks>
    /// See 9.3 of the Universal Serial Bus (USB) specification (<see href="www.usb.org"/>)
    /// The direction of a USB transfer is determined from the host side and not the target USB device. 
    /// In an IN transfer, data moves from the device to the host. In an OUT transfer, data moves from the host to the device.
    /// </remarks>
    public enum RequestDirection
    {
        /// <summary>
        /// Indicates an IN transfer from the device to the host.
        /// </summary>
        In = 1,

        /// <summary>
        /// Indicates an OUT transfer from the host to the device.
        /// </summary>
        Out = 0
    }
}
