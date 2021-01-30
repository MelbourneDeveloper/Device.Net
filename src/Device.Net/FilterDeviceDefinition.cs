using System;

namespace Device.Net
{
    public sealed class FilterDeviceDefinition
    {
        public FilterDeviceDefinition(
            uint? vendorId = null,
            uint? productId = null,
            ushort? usagePage = null,
            string label = null,
             Guid? classGuid = null)
        {
            VendorId = vendorId;
            ProductId = productId;
            UsagePage = usagePage;
            Label = label;
            ClassGuid = classGuid;
        }

        public uint? VendorId { get; }

        /// <summary>
        /// Product Id
        /// </summary>
        public uint? ProductId { get; }

        /// <summary>
        /// Used to further filter down device definitions on some platforms
        /// </summary>
        public ushort? UsagePage { get; }

        /// <summary>
        /// Not used for filtering. Only used to give a meaningful name to the filter that is carried on to the device definition
        /// </summary>
        public string Label { get; }


        public Guid? ClassGuid { get; }
    }
}
