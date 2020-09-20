namespace Device.Net
{
    public sealed class FilterDeviceDefinition
    {
        public uint? VendorId { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        public uint? ProductId { get; set; }

        /// <summary>
        /// Used to further filter down device definitions on some platforms
        /// </summary>
        public ushort? UsagePage { get; set; }

        /// <summary>
        /// Not used for filtering. Only used to give a meaningful name to the filter that is carried on to the device definition
        /// </summary>
        public string Label { get; set; }
    }
}
