
namespace Usb.Net
{
    public interface ISetupPacket
    {
         byte RequestType { get; set; }

        /// <summary>
        /// This field specifies the particular request. The Type bits in the bmRequestType field modify the meaning of this field. This specification defines values for the bRequest field only when the bits are reset to zero, indicating a standard request.
        /// </summary>
        byte Request { get; set; }

        /// <summary>
        /// The contents of this field vary according to the request. It is used to pass a parameter to the device, specific to the request.
        /// </summary>
        short Value { get; set; }

        /// <summary>
        /// The contents of this field vary according to the request. It is used to pass a parameter to the device, specific to the request.
        /// </summary>
        short Index { get; set; }

        /// <summary>
        /// This field specifies the length of the data transferred during the second phase of the control transfer. The direction of data transfer (host-to-device or device-to-host) is indicated by the Direction bit of the <see cref="RequestType"/> field. If this field is zero, there is no data transfer phase. On an input request, a device must never return more data than is indicated by the wLength value; it may return less. On an output request, wLength will always indicate the exact amount of data to be sent by the host. Device behavior is undefined if the host should send more data than is specified in wLength.
        /// </summary>
        short Length { get; set; }
    }
}
