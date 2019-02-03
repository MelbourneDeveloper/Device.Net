using System;

using AppKit;
using Device.Net.LibUsb;
using Foundation;
using Usb.Net.Sample;

namespace Device.Net.MacOSLibUsbSample
{
    public partial class ViewController : NSViewController
    {
        TrezorExample TrezorExample = new TrezorExample();

        public ViewController(IntPtr handle) : base(handle)
        {

        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LibUsbUsbDeviceFactory.Register();

            await TrezorExample.InitializeTrezorAsync();
            var buffer = await TrezorExample.WriteAndReadFromDeviceAsync();
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
