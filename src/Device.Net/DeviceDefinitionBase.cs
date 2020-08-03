namespace Device.Net
{
    //https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/hid-usages#usage-id

    /// <summary>
    /// A definition for a device.
    /// </summary>
    public abstract class DeviceDefinitionBase
    {
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
        /// Used to further filter down device definitions on some platforms
        /// </summary>
        public ushort? UsagePage { get; set; }

        public override string ToString() => $"VendorId: {VendorId} Product Id: {ProductId}";
    }
}