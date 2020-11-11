namespace Usb.Net
{
    public class SetupPacket
    {
        /// <summary>
        /// Size of <see cref="SetupPacket"/> to be used in byte arrays.
        /// </summary>
        // this is the "byte" size of the properties below 
        public const int SetupPacketSize = 1 + 1 + 2 + 2 + 2;

        #region Public Properties
        public UsbDeviceRequestType RequestType { get; }
        public byte Request { get; }
        public ushort Value { get; }
        public ushort Index { get; }
        public ushort Length { get; }
        #endregion

        #region Constructors
        public SetupPacket(
            UsbDeviceRequestType requestType,
            byte request,
            ushort value = 0,
            ushort index = 0,
            ushort length = 0)
        {
            RequestType = requestType;
            Request = request;
            Value = value;
            Index = index;
            Length = length;
        }
        #endregion

    }
}
