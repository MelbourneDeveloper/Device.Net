namespace Android.Hardware.Usb
{
    //
    // Summary:
    //     Enumerates values returned by several types.
    //
    // Remarks:
    //     Portions of this page are modifications based on work created and shared by the
    //     Android Open Source Project and used according to terms described in the Creative
    //     Commons 2.5 Attribution License.
    public enum UsbClass
    {
        //
        // Summary:
        //     USB class indicating that the class is determined on a per-interface basis.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        PerInterface = 0,
        //
        // Summary:
        //     USB class for audio devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Audio = 1,
        //
        // Summary:
        //     USB class for communication devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Comm = 2,
        //
        // Summary:
        //     USB class for human interface devices (for example, mice and keyboards).
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Hid = 3,
        //
        // Summary:
        //     USB class for physical devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Physica = 5,
        //
        // Summary:
        //     USB class for still image devices (digital cameras).
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        StillImage = 6,
        //
        // Summary:
        //     USB class for printers.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Printer = 7,
        //
        // Summary:
        //     USB class for mass storage devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        MassStorage = 8,
        //
        // Summary:
        //     USB class for USB hubs.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Hub = 9,
        //
        // Summary:
        //     USB class for CDC devices (communications device class).
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        CdcData = 10,
        //
        // Summary:
        //     USB class for content smart card devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        CscId = 11,
        //
        // Summary:
        //     USB class for content security devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        ContentSec = 13,
        //
        // Summary:
        //     USB class for video devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Video = 14,
        //
        // Summary:
        //     USB class for wireless controller devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        WirelessController = 224,
        //
        // Summary:
        //     USB class for wireless miscellaneous devices.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Misc = 239,
        //
        // Summary:
        //     Application specific USB class.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        AppSpec = 254,
        //
        // Summary:
        //     Vendor specific USB class.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        VendorSpec = 255
    }
}