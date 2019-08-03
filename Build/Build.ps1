
Set-ExecutionPolicy -ExecutionPolicy Bypass

#TODO: Unhard code these
$msbuildToolsPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin"
$version = "3.0.0"

$releaseNotes = "Breaking changes. See https://github.com/MelbourneDeveloper/Device.Net/projects/8"

$deviceNetTitle = "Device.Net"
$deviceNetSummary = "Cross platform C# library for talking to connected devices in a uniform way through dependency injection."

$hidNetTitle = "Hid.Net"
$hidNetSummary = "Cross platform Hid  library. Enumerate Hid (USB) devices and read and write to/from them."

$usbNetTitle = "Usb.Net"
$usbNetSummary = "A cross platform USB library written in C#"

$deviceNetLibUsbTitle = "Device.Net.LibUsb"
$deviceNetLibUsbSummary = "Device.Net based library for USB access on Linux, MacOS and Windows. Use Usb.Net, and Hid.Net for Windows, UWP, and Android support. The base interfaces are the same so the code is compatible on all platforms"

cls

#Clean the solution
function Clean()
{
    git clean -x -f -d
}

function DoDeviceNet()
{
#Get paths ready
$deviceNetDllName = "Device.Net.dll"
$deviceNetUWPDllName = "Device.Net.UWP.dll"

$deviceNetNuspecPath = $basepath + "\src\Device.Net\obj\Release\Device.Net." + $version + ".nuspec"

$deviceNetStandardDllRelativePath = -join("\src\Device.Net\bin\Release\netstandard2.0\", $deviceNetDllName) 
$deviceNetStandardDllPath = Join-Path -Path $basepath -ChildPath $deviceNetStandardDllRelativePath

$deviceNetUWPDllRelativePath = -join("\src\Device.Net.UWP\bin\Release\", $deviceNetUWPDllName)
$deviceNetUWPDllPath = Join-Path -Path $basepath -ChildPath $deviceNetUWPDllRelativePath

#Get the Device.Net Nuspec document
[xml]$myXML = Get-Content $deviceNetNuspecPath

$summaryElement = $myXML.CreateElement("summary")
$summaryElement.InnerText = $deviceNetSummary
$myXML.package.metadata.AppendChild($summaryElement)

$releaseNotesElement = $myXML.CreateElement("releaseNotes")
$releaseNotesElement.InnerText = $releaseNotes
$myXML.package.metadata.AppendChild($releaseNotesElement)

$titleElement = $myXML.CreateElement("title")
$titleElement.InnerText = $deviceNetTitle
$myXML.package.metadata.AppendChild($titleElement)

#This seems horribe but what you gonna do?
$metadataElement = $myXML.GetElementsByTagName("metadata")[0]
$licenseUrlElement = $myXML.GetElementsByTagName("licenseUrl")[0]
$metadataElement.RemoveChild($licenseUrlElement)

#Create the UWP Device.Net.UWP file element
$deviceNetUWPFile = $myXML.CreateElement("file")
$deviceNetUWPFile.SetAttribute("src", $deviceNetUWPDllPath);
$deviceNetUWPFile.SetAttribute("target",  -join("lib\uap10.0\", $deviceNetUWPDllName) );
$myXML.package.files.AppendChild($deviceNetUWPFile)

#Create the UWP Device.Net file element
$deviceNetUWPFile = $myXML.CreateElement("file")
$deviceNetUWPFile.SetAttribute("src", $deviceNetStandardDllPath);
$deviceNetUWPFile.SetAttribute("target",  -join("lib\uap10.0\", $deviceNetDllName) );
$myXML.package.files.AppendChild($deviceNetUWPFile)

#Save the Device.Net nuspec file
$myXML.Save($deviceNetNuspecPath)

#Pack the Device.Net NuGet package
$deviceNetNugetPackExpression = $basePath + "\Build\nuget pack " + $deviceNetNuspecPath + " -OutputDirectory " + $outputPath
Invoke-Expression $deviceNetnugetPackExpression
}

function DoHidNet()
{
$hidNetDllName = "Hid.Net.dll"
$hidNetUWPDllName = "Hid.Net.UWP.dll"

$hidNetNuspecPath = $basepath + "\src\Hid.Net\obj\Release\Hid.Net." + $version + ".nuspec"
$hidNetStandardDllPath = $basepath + "\src\Hid.Net\bin\Release\netstandard2.0\" + $hidNetDllName
$hidNetUWPDllPath = $basepath + "\src\Hid.Net.UWP\bin\Release\" + $hidNetUWPDllName

#Get the Device.Net Nuspec document
[xml]$myXML = Get-Content $hidNetNuspecPath

$summaryElement = $myXML.CreateElement("summary")
$summaryElement.InnerText = $hidNetSummary
$myXML.package.metadata.AppendChild($summaryElement)

$titleElement = $myXML.CreateElement("title")
$titleElement.InnerText = $hidNetTitle
$myXML.package.metadata.AppendChild($titleElement)

#This seems horribe but what you gonna do?
$metadataElement = $myXML.GetElementsByTagName("metadata")[0]
$licenseUrlElement = $myXML.GetElementsByTagName("licenseUrl")[0]
$metadataElement.RemoveChild($licenseUrlElement)

$releaseNotesElement = $myXML.CreateElement("releaseNotes")
$releaseNotesElement.InnerText = $releaseNotes
$myXML.package.metadata.AppendChild($releaseNotesElement)

#Create the UWP Hid.Net.UWP file element
$hidNetUWPFile = $myXML.CreateElement("file")
$hidNetUWPFile.SetAttribute("src", $hidNetUWPDllPath);
$hidNetUWPFile.SetAttribute("target",  -join("lib\uap10.0\", $hidNetUWPDllName) );
$myXML.package.files.AppendChild($hidNetUWPFile)

#Create the UWP Hid.Net file element
$hidNetUWPFile = $myXML.CreateElement("file")
$hidNetUWPFile.SetAttribute("src", $hidNetStandardDllPath);
$hidNetUWPFile.SetAttribute("target",  -join("lib\uap10.0\", $hidNetDllName) );
$myXML.package.files.AppendChild($hidNetUWPFile)

#Save the Hid.Net nuspec file
$myXML.Save($hidNetNuspecPath)

#Pack the Hid.Net NuGet package
$hidNetNugetPackExpression = $basePath + "\Build\nuget pack " + $hidNetNuspecPath + " -OutputDirectory " + $outputPath
Invoke-Expression $hidNetNugetPackExpression
}

function DoUsbNet()
{
$usbNetDllName = "Usb.Net.dll"
$usbNetUWPDllName = "Usb.Net.UWP.dll"
$usbNetAndroidDllName = "Usb.Net.Android.dll"

$usbNetNuspecPath = $basepath + "\src\Usb.Net\obj\Release\Usb.Net." + $version + ".nuspec"
$usbNetStandardDllPath = $basepath + "\src\Usb.Net\bin\Release\netstandard2.0\" + $usbNetDllName
$usbNetUWPDllPath = $basepath + "\src\Usb.Net.UWP\bin\Release\" + $usbNetUWPDllName
$usbNetAndroidDllPath = $basepath + "\src\Usb.Net.Android\bin\Release\" + $usbNetAndroidDllName

#Get the Usb.Net Nuspec document
[xml]$myXML = Get-Content $usbNetNuspecPath

$titleElement = $myXML.CreateElement("title")
$titleElement.InnerText = $usbNetTitle
$myXML.package.metadata.AppendChild($titleElement)

#This seems horribe but what you gonna do?
$metadataElement = $myXML.GetElementsByTagName("metadata")[0]
$licenseUrlElement = $myXML.GetElementsByTagName("licenseUrl")[0]
$metadataElement.RemoveChild($licenseUrlElement)

$summaryElement = $myXML.CreateElement("summary")
$summaryElement.InnerText = $usbNetSummary
$myXML.package.metadata.AppendChild($summaryElement)

$releaseNotesElement = $myXML.CreateElement("releaseNotes")
$releaseNotesElement.InnerText = $releaseNotes
$myXML.package.metadata.AppendChild($releaseNotesElement)

#Create the UWP Usb.Net.UWP file element
$usbNetUWPFile = $myXML.CreateElement("file")
$usbNetUWPFile.SetAttribute("src", $usbNetUWPDllPath);
$usbNetUWPFile.SetAttribute("target",  -join("lib\uap10.0\", $usbNetUWPDllName) );
$myXML.package.files.AppendChild($usbNetUWPFile)

#Create the UWP Usb.Net file element
$usbNetUWPFile = $myXML.CreateElement("file")
$usbNetUWPFile.SetAttribute("src", $usbNetStandardDllPath);
$usbNetUWPFile.SetAttribute("target",  -join("lib\uap10.0\", $usbNetDllName) );
$myXML.package.files.AppendChild($usbNetUWPFile)

#Create the Android Usb.Net.Android file element
$hidNetAndroidFile = $myXML.CreateElement("file")
$hidNetAndroidFile.SetAttribute("src", $usbNetAndroidDllPath);
$hidNetAndroidFile.SetAttribute("target",  -join("lib\MonoAndroid\", $usbNetAndroidDllName) );
$myXML.package.files.AppendChild($hidNetAndroidFile)

#Create the Android Usb.Net file element
$hidNetAndroidFile = $myXML.CreateElement("file")
$hidNetAndroidFile.SetAttribute("src", $usbNetStandardDllPath);
$hidNetAndroidFile.SetAttribute("target",  -join("lib\MonoAndroid\", $usbNetDllName) );
$myXML.package.files.AppendChild($hidNetAndroidFile)

#Save the Usb.Net nuspec file
$myXML.Save($usbNetNuspecPath)

#Pack the Usb.Net NuGet package
$usbNetNugetPackExpression = $basePath + "\Build\nuget pack " + $usbNetNuspecPath + " -OutputDirectory " + $outputPath
Invoke-Expression $usbNetNugetPackExpression
}


function DoDeviceNetLibUsb()
{
$deviceNetLibUsbNetNuspecPath = $basepath + "\src\Device.Net.LibUsb\obj\Release\Device.Net.LibUsb." + $version + ".nuspec"

[xml]$myXML = Get-Content $deviceNetLibUsbNetNuspecPath

$titleElement = $myXML.CreateElement("title")
$titleElement.InnerText = $deviceNetLibUsbTitle
$myXML.package.metadata.AppendChild($titleElement)

#This seems horribe but what you gonna do?
$metadataElement = $myXML.GetElementsByTagName("metadata")[0]
$licenseUrlElement = $myXML.GetElementsByTagName("licenseUrl")[0]
$metadataElement.RemoveChild($licenseUrlElement)

$summaryElement = $myXML.CreateElement("summary")
$summaryElement.InnerText = $deviceNetLibUsbSummary
$myXML.package.metadata.AppendChild($summaryElement)

$releaseNotesElement = $myXML.CreateElement("releaseNotes")
$releaseNotesElement.InnerText = $releaseNotes
$myXML.package.metadata.AppendChild($releaseNotesElement)

#Save the Device.Net.LibUsb nuspec file
$myXML.Save($deviceNetLibUsbNetNuspecPath)

#Pack the Device.Net.LibUsb NuGet package
$deviceNetLibUsbNugetPackExpression = $basePath + "\Build\nuget pack " + $deviceNetLibUsbNetNuspecPath + " -OutputDirectory " + $outputPath
Invoke-Expression $deviceNetLibUsbNugetPackExpression
}

$scriptPath = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition) 

$msbuildPath = Join-Path -Path $msbuildToolsPath -ChildPath "MSBuild.exe"

$nugetpath = Join-Path -Path $scriptPath -ChildPath "nuget.exe"

$solutionFilePath = Join-Path -Path $basepath -ChildPath "\src\Device.Net - Libraries.sln"

#Get the path of the repo and go to it
$basepath = Split-Path -parent $scriptPath
cd $basepath

$outputPath = Join-Path -Path $basepath -ChildPath "\NuGets"

Clean

#Make sure the output folder exists
If(!(test-path $outputPath))
{
    New-Item -ItemType Directory -Force -Path $outputPath
}

#Restore NuGet packages for the solution
$restoreNuGetsExpression = $nugetpath + " restore " + """" + $solutionFilePath + """" + " -MSBuildPath " + """" + $msbuildToolsPath + """"
Invoke-Expression $restoreNuGetsExpression

#Build everything
[Array]$arguments = $solutionFilePath,  "/m", "/t:Build", "/p:Configuration=Release";
& $msbuildPath $arguments;

DoDeviceNet

DoHidNet

DoUsbNet

DoDeviceNetLibUsb