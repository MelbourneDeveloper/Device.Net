git clean -x -f -d

"c:\temp\nuget restore" src/Device.Net.Pipelines.sln

msbuild src/Device.Net.Pipelines.sln /property:Configuration=Release

"c:\temp\nuget pack" Build/NuSpecs/Device.Net.nuspec -Symbols -SymbolPackageFormat snupkg
"c:\temp\nuget pack" Build/NuSpecs/Device.Net.LibUsb.nuspec -Symbols -SymbolPackageFormat snupkg
"c:\temp\nuget pack" Build/NuSpecs/Hid.Net.nuspec -Symbols -SymbolPackageFormat snupkg
"c:\temp\nuget pack" Build/NuSpecs/SerialPort.Net.nuspec -Symbols -SymbolPackageFormat snupkg
"c:\temp\nuget pack" Build/NuSpecs/Usb.Net.nuspec -Symbols -SymbolPackageFormat snupkg

