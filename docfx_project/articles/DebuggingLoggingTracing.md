It is important to understand how to use the logging and tracing functionality. If this doesn't resolve your issue, you should clone this repo and add references to the necessary projects to debug directly. See [here](BuildIssues.cs) for building the solution. If the issue still cannot be resolved, please log a GitHub issue, but please make sure you've followed these steps before reporting an issue.

## Logging
Device.Net uses the ASP .NET Core logging infrastructure. It would help if you read about this system [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-5.0). All devices accept an [`ILogger`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger?view=dotnet-plat-ext-5.0), and device factories accept an [`ILoggerFactory`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory?view=dotnet-plat-ext-5.0). When factories create new devices, they inject a logger into each one. With 3rd party libraries, you can write to file or send telemetry to a cloud service like [Application Insights](https://azure.microsoft.com/en-au/services/monitor/). We highly recommend that you send telemetry data to a central location so that you can diagnose issues with your clients' software and devices without having access to their computer.

## Debug Tracing
If you set the minimum level of the logs to `Trace` or below, logs will output all data transferred to/from the device. The Windows sample logs to the debug window. This is how we initialize logging to the debug window:

[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/e7f3711e90daec6de39a42d8a468fc21030c1692/src/Usb.Net.WindowsSample/Program.cs#L40)
```cs
_loggerFactory = LoggerFactory.Create((builder) =>
{
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Trace);
});
```

When reporting issues, please look at the Debug output window, copy the output, and paste it into the GitHub issue. 

## Advanced Use
Check out ASP.NET Core logging frameworks such as [Serilog](https://github.com/serilog/serilog-aspnetcore), [log4net](https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore), [NLog](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-3) and the [Application Insights SDK](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core). 

## I Can't See My Device
Run the Windows sample. You should see your device's path appear at the top of the console. If it does not, check the logs and turn on break on all exceptions (see below). The log should tell you what went wrong when the library attempted to detect the device.

![Image](../images/WindowsSample.png)

## Before Reporting an Issue
If anything goes wrong in the framework, the app should throw an exception. To catch Exceptions and obtain a stack trace, you should turn on break on all exceptions in Visual Studio to catch the exact location where the problem is occurring. Please see [this article](https://christianfindlay.com/2019/07/14/visual-studio-break-on-all-exceptions/) . Please take screenshots of where the Exception issue is occurring and include those in any GitHub reports.