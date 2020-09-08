namespace Device.Net.UWP
{
    public static class AqsHelpers
    {
        public const string InterfaceEnabledPart = "AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
        private const string VendorFilterName = "System.DeviceInterface.Hid.VendorId";
        private const string ProductFilterName = "System.DeviceInterface.Hid.ProductId";

        public static string GetVendorPart(uint? vendorId)
        {
            string vendorPart = null;
            if (vendorId.HasValue) vendorPart = $"AND {VendorFilterName}:={vendorId.Value}";
            return vendorPart;
        }

        public static string GetProductPart(uint? productId)
        {
            string productPart = null;
            if (productId.HasValue) productPart = $"AND {ProductFilterName}:={productId.Value}";
            return productPart;
        }

    }
}
