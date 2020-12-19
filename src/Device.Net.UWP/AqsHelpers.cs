namespace Device.Net.UWP
{
    public static class AqsHelpers
    {
        public const string InterfaceEnabledPart = "System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
        private const string HidVendorFilterName = "System.DeviceInterface.Hid.VendorId";
        private const string HidProductFilterName = "System.DeviceInterface.Hid.ProductId";

        private const string VendorFilterName = "System.DeviceInterface.WinUsb.UsbVendorId";
        private const string ProductFilterName = "System.DeviceInterface.WinUsb.UsbProductId";

        public static string GetVendorPart(uint? vendorId, DeviceType deviceType)
        {
            string vendorPart = null;
            if (vendorId.HasValue) vendorPart = $"{ (deviceType == DeviceType.Hid ? HidVendorFilterName : VendorFilterName)}:={vendorId.Value}";
            return vendorPart;
        }

        public static string GetProductPart(uint? productId, DeviceType deviceType)
        {
            string productPart = null;
            if (productId.HasValue) productPart = $"{(deviceType == DeviceType.Hid ? HidProductFilterName : ProductFilterName) }:={productId.Value}";
            return productPart;
        }
    }
}
