namespace Device.Net
{
    public abstract class ConnectedDeviceDefinitionBase : DeviceDefinitionBase
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
        /// 
        /// </summary>
        public ushort? Usage { get; set; }

        /// <summary>
        /// Device version number
        /// </summary>
        public ushort? VersionNumber { get; set; }

        /// <summary>
        /// The maximum size of data to be written to the device
        /// </summary>
        public int? WriteBufferSize { get; set; }

        /// <summary>
        /// The maximum size of data to be read from the device
        /// </summary>
        public int? ReadBufferSize { get; set; }
    }
}
