namespace Usb.Net
{
    /// <summary>
    /// Defines constants that indicate the type of USB control transfer: standard, class, or vendor.
    /// </summary>
    public enum UsbControlTransferType
    {
        /// <summary>
        /// Indicates a class-specific USB control request described by a specific device class specification.
        /// </summary>
        Class = 1,

        /// <summary>
        /// Indicates a standard USB control request that the app can send to obtain information about the device, its configurations, interfaces, and endpoints.
        /// The standard requests are described in section 9.4 of the Universal Serial Bus(USB) specification(www.usb.org).
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Indicates a vendor-specified USB control request and depends on the requests supported by the device.
        /// </summary>
        Vendor = 2
    }
}
