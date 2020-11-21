// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Usb.Net
{
    /// <summary>
    /// Defines the type of USB device request.
    /// </summary>
    /// <remarks>
    /// See 9.3 of the Universal Serial Bus (USB) specification (<see cref="www.usb.org"/>)
    /// </remarks>
    public enum RequestType
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
