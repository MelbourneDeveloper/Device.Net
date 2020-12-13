If the framework does not behave as expected, it is important to understand how to use the logging and tracing functionality. If this doesn't resolve your issue, you should clone this repo, and add references to the necessary projects so that can debug directly. See [here](https://github.com/MelbourneDeveloper/Device.Net/wiki/Build-Issues) for building the solution. If the issue still cannot be resolved, then please log a GitHub issue, but please make sure you've followed these steps before reporting an issue.

## Debug Tracing
All devices accept an [ILogger](https://github.com/MelbourneDeveloper/Device.Net/blob/master/src/Device.Net/ILogger.cs), and an [ITracer](https://github.com/MelbourneDeveloper/Device.Net/blob/master/src/Device.Net/ITracer.cs) as arguments. These are used by the devices to log events that might occur during the process of connecting to and communicating with the device. The simplest logger is [DebugLogger](https://github.com/MelbourneDeveloper/Device.Net/blob/master/src/Device.Net/DebugLogger.cs), and the simplest tracer is [DebugTracer](https://github.com/MelbourneDeveloper/Device.Net/blob/master/src/Device.Net/DebugTracer.cs). The Windows sample uses these [here](https://github.com/MelbourneDeveloper/Device.Net/blob/7a4e566603238c915e3b311cce1ee3f48aa9c480/src/Usb.Net.WindowsSample/Program.cs#L23). Both these classes will log formatted information to the Debug output Window. When reporting issues, please look at the Debug output window, copy the output, and paste it in to the GitHub issue.

## Advanced Use
If logging to the Debug output window is inadequate, please implement the ILogger, and ITracer interfaces and implement your own tracing information. You may want to log to an external telemetry system so that you can track issues in your user's usage of the framework.

## Tracing
Tracing is a very important feature because it will allow you to see what data is being written to the device, and what is being read from the device. You will be able to use this information to compare with other USB sniffing tools so that you can ensure that you are sending the same data to the device that other software is sending. You can use this to validate that the data transfer is correct.

## I Can't See My Device
Run the Windows sample. You should see your device's path appear at the top of the console. If it does not, check the logs, and turn on break on all exceptions (see below). The log should tell you what went wrong when the library attempted to detect the device.

![Image](https://github.com/MelbourneDeveloper/Device.Net/blob/master/Images/WindowsSample.png)

## Before Reporting an Issue
If anything goes wrong in the framework, an Exception should be thrown. In order to catch Exceptions and obtain a stack trace, you should turn on break on all exceptions in Visual Studio to catch the exact location where the problem is occurring. Please see [this article](https://christianfindlay.com/2019/07/14/visual-studio-break-on-all-exceptions/) . Please take screenshots of where the Exception issue occurring and include those in any GitHub reports.