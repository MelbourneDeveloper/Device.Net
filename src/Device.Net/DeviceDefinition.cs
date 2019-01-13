namespace Device.Net
{
    //https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/hid-usages#usage-id

    /// <summary>
    /// A definition for a device.
    /// </summary>
    public abstract class DeviceDefinitionBase
    {
        /// <summary>
        /// The name of the device product according to the Manufacturer
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Name of the device's manufacturer
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Unique serial number of the physical device
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public uint? VendorId { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        public uint? ProductId { get; set; }

        /// <summary>
        /// Freeform tag to be used as needed
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The type of device interface
        /// </summary>
        public DeviceType? DeviceType { get; set; }

        /// <summary>
        /// The maximum size of data to be written to the device
        /// </summary>
        public int? WriteBufferSize { get; set; }

        /// <summary>
        /// The maximum size of data to be read from the device
        /// </summary>
        public int? ReadBufferSize { get; set; }

        /// <summary>
        /// Used to further filter down device definitions on some platforms
        /// </summary>
        public ushort? UsagePage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort? Usage { get; set; }

        /// <summary>
        /// Device version number
        /// </summary>
        public ushort? VersionNumber { get; set; }
    }
}