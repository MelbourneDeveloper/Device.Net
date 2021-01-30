There are samples in the Samples folder inside the main solution of this repo. Please clone this repo to see the samples. This should be the first place to look. The samples have direct enumerating and device listening. The examples there are for talking to a Trezor Hardwarewallet. Most of the code there is shared across all platforms. Please open the solution that is appropriate for your case, or platform. You can see all the solutions (sln files) [here](https://github.com/MelbourneDeveloper/Device.Net/tree/master/src). Note: if you open the "All" solution, some platforms may not open, compile or run.

See the [Build Issues](https://github.com/MelbourneDeveloper/Device.Net/wiki/Build-Issues) section if you have trouble getting a sample to run.

Here are some other repos that also use Usb.Net or Hid.Net. These should help you to understand how the libraries can be used to connect to various devices. The libraries are for cryptocurrency hardwarewallets. Your best bet for connecting to a device and using it is to clone one of these repos and modify it to suit your needs.

| Platform | Device Types |
| ------------- |:-------------:|
| [Trezor.Net](https://github.com/MelbourneDeveloper/Trezor.Net) | UWP, Android, Console - .NET Framework |
| [Ledger.Net](https://github.com/MelbourneDeveloper/Ledger.Net) | UWP, Console - .NET Core |
| [Ledger .NET API](https://github.com/LedgerHQ/ledger-dotnet-api) | Console - .NET Core |
| [KeepKey.Net](https://github.com/MelbourneDeveloper/KeepKey.Net) |  UWP, Android, .NET Framework |