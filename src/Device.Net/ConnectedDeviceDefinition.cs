using System;

namespace Device.Net
{

    /// <summary>
    /// Represents the definition of a device that has been physically connected and has a DeviceId
    /// </summary>
    public class ConnectedDeviceDefinition
    {
        #region Public Properties

        /// <summary>
        /// The device Id or path specific to the platform for the device
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// The type of device interface
        /// </summary>
        public DeviceType DeviceType { get; }

        public Guid? ClassGuid { get; }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public uint? VendorId { get; }

        /// <summary>
        /// Product Id
        /// </summary>
        public uint? ProductId { get; }

        /// <summary>
        /// Freeform tag to be used as needed
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Used to further filter down device definitions on some platforms
        /// </summary>
        public ushort? UsagePage { get; }

        /// <summary>
        /// The name of the device product according to the Manufacturer
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// Name of the device's manufacturer
        /// </summary>
        public string Manufacturer { get; }

        /// <summary>
        /// Unique serial number of the physical device
        /// </summary>
        public string SerialNumber { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort? Usage { get; }

        /// <summary>
        /// Device version number
        /// </summary>
        public ushort? VersionNumber { get; }

        /// <summary>
        /// The maximum size of data to be written to the device
        /// </summary>
        public int? WriteBufferSize { get; }

        /// <summary>
        /// The maximum size of data to be read from the device
        /// </summary>
        public int? ReadBufferSize { get; }
        #endregion

        #region Constructor
        public ConnectedDeviceDefinition(
            string deviceId,
            DeviceType deviceType,
            uint? vendorId = null,
            uint? productId = null,
            string productName = null,
            string manufacturer = null,
            string serialNumber = null,
            ushort? usage = null,
            ushort? usagePage = null,
            ushort? versionNumber = null,
            int? writeBufferSize = null,
            int? readBufferSize = null,
            string label = null,
            Guid? classGuid = null
           )
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            ClassGuid = classGuid;
            DeviceId = deviceId;
            VendorId = vendorId;
            ProductId = productId;
            DeviceType = deviceType;
            ProductName = productName;
            Manufacturer = manufacturer;
            SerialNumber = serialNumber;
            Usage = usage;
            UsagePage = usagePage;
            VersionNumber = versionNumber;
            WriteBufferSize = writeBufferSize;
            ReadBufferSize = readBufferSize;
            Label = label;
        }
        #endregion

        public override string ToString() => $"Device Id: {DeviceId} Label: {Label} Vid: {VendorId} Pid: {ProductId}\r\nRead Buffer Size: {ReadBufferSize} Write Buffer Size: {WriteBufferSize}\r\nManufacturer: {Manufacturer} Product Name: {ProductName}";
    }
}