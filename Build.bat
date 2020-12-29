git clean -x -f -d

"c:\temp\nuget" restore src/Device.Net.Pipelines.sln

dotnet test src/Device.Net.UnitTests/Device.Net.UnitTests.csproj --filter TestCategory!=NotPipelineReady

msbuild src/Device.Net.Pipelines.sln /property:Configuration=Release

"c:\temp\nuget" pack Build/NuSpecs/Device.Net.nuspec
"c:\temp\nuget" pack Build/NuSpecs/Device.Net.LibUsb.nuspec
"c:\temp\nuget" pack Build/NuSpecs/Hid.Net.nuspec
"c:\temp\nuget" pack Build/NuSpecs/SerialPort.Net.nuspec
"c:\temp\nuget" pack Build/NuSpecs/Usb.Net.nuspec

