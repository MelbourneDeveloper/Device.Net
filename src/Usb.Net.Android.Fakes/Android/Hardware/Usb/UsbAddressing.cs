namespace Android.Hardware.Usb
{
    //
    // Summary:
    //     Enumerates values returned by several types and taken as a parameter of several
    //     methods of Android.Hardware.Usb.UsbDeviceConnection.
    //
    // Remarks:
    //     Portions of this page are modifications based on work created and shared by the
    //     Android Open Source Project and used according to terms described in the Creative
    //     Commons 2.5 Attribution License.
    public enum UsbAddressing
    {
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Out = 0,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        XferControl = 0,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        XferIsochronous = 1,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        XferBulk = 2,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        XferInterrupt = 3,
        //
        // Summary:
        //     Bitmask used for extracting the Android.Hardware.Usb.UsbEndpointtype from its
        //     address field.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        XferTypeMask = 3,
        //
        // Summary:
        //     Bitmask used for extracting the Android.Hardware.Usb.UsbEndpointnumber its address
        //     field.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        NumberMask = 15,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        In = 128,
        //
        // Summary:
        //     Bitmask used for extracting the Android.Hardware.Usb.UsbEndpointdirection from
        //     its address field.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        DirMask = 128
    }
}

