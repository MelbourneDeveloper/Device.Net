namespace Device.Net
{
    public class DeviceDefinition
    {
        /// <summary>
        /// Platform specific, unique Id for the device
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public uint VendorId { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        public uint ProductId { get; set; }

        /// <summary>
        /// Freeform tag to be used as needed
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The type of device interface
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// The maximum size of data to be written to the device
        /// </summary>
        public int WriteBufferSize { get; set; }

        /// <summary>
        /// The maximum size of data to be read from the device
        /// </summary>
        public int ReadBufferSize { get; set; }
    }
}