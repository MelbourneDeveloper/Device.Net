
namespace Usb.Net
{
    public class UsbDeviceRequestType
    {
        /// <summary>
        /// Gets the type of the USB device request.
        /// </summary>
        public RequestType Type { get; }

        /// <summary>
        /// Gets the direction of the USB device request.
        /// </summary>
        public RequestDirection Direction { get; }

        /// <summary>
        /// Gets the direction of the USB device request.
        /// </summary>
        public RequestRecipient Recipient { get; }

        public UsbDeviceRequestType(
            RequestDirection direction,
            RequestType type,
            RequestRecipient recipient)
        {
            Direction = direction;
            Type = type;
            Recipient = recipient;
        }

        public byte ToByte()
        {
            return (byte)(
            ((byte)Direction << 7) +
            ((byte)Type << 5) +
            (byte)Recipient);
        }

        public override string ToString() => $"RequestType: {Type} RequestDirection: {Direction} RequestRecipient: {Recipient}";
    }
}
