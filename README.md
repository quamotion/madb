| build | coverage |
|-------|----------|
| [![Build Status](https://ci.appveyor.com/api/projects/status/github/quamotion/madb)](https://ci.appveyor.com/project/qmfrederik/madb/) | [![codecov.io](https://codecov.io/github/quamotion/madb/coverage.svg?branch=master)](https://codecov.io/github/quamotion/madb?branch=master)

# A .NET client for adb, the Android Debug Bridge (SharpAdbClient)

SharpAdbClient is a .NET library that allows .NET applications to communicate with Android devices. 
It provides a .NET implementation of the `adb` protocol, giving more flexibility to the developer than launching an 
`adb.exe` process and parsing the console output.

## Installation
To install SharpAdbClient install the [SharpAdbClient NuGetPackage](https://www.nuget.org/packages/SharpAdbClient). If you're
using Visual Studio, you can run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console):

```
PM> Install-Package SharpAdbClient
```

## Getting Started

All of the adb functionality is exposed through the `SharpAdbClient.AdbClient` class. You can create your own instance of that class,
or just use the instance we provide for you at `SharpAdbClient.AdbClient.Instance`.

This class provides various methods that allow you to interact with Android devices.

### Starting the `adb` server
SharpAdbClient does not communicate directly with your Android devices, but uses the `adb.exe` server process as an intermediate. Before you can connect to your Android device, you must first start the `adb.exe` server.

You can do so by either running `adb.exe` yourself (it comes as a part of the ADK, the Android Development Kit), or you can use the `AdbServer.StartServer` method like this:

```
AdbServer.StartServer(@"C:\Program Files (x86)\android-sdk\platform-tools\adb.exe", restartServerIfNewer: false);
```

### List all Android devices currently connected
To list all Android devices that are connected to your PC, you can use the following code:

```
var devices = AdbClient.Instance.GetDevices();

foreach(var device in devices)
{
    Console.WriteLine(device.Name);
}
```

### Subscribe for events when devices connect/disconnect
To receive notifications when devices connect to or disconnect from your PC, you can use the `DeviceMonitor` class:

```
void Test()
{
    var monitor = new DeviceMonitor(new AdbSocket());
    monitor.DeviceConnected += this.OnDeviceConnected;
    monitor.Start();
}

void OnDeviceConnected(object sender, DeviceDataEventArgs e)
{
    Console.WriteLine($"The device {e.Device.Name} has connected to this PC");
}
```

### Manage applications
To install or uninstall applications, you can use the `ApplicationManager` class:

```
void InstallApplication()
{
    var device = AdbClient.Instance.GetDevices().First();
    PackageManager manager = new PackageManager(device);
    manager.InstallPackage(@"C:\Users\me\Documents\mypackage.apk", reinstall: false);
}
```

### Send or receive files
To send files to or receive files from your Android device, you can use the `SyncService` class like this:

```
void DownloadFile()
{
    var device = AdbClient.Instance.GetDevices().First();
    
    using (SyncService service = new SyncService(new AdbSocket(), device))
    using (Stream stream = File.OpenWrite(@"C:\MyFile.txt"))
    {
        service.Pull("/data/MyFile.txt", stream, null, CancellationToken.None);     
    }
}

void UploadFile()
{
    var device = AdbClient.Instance.GetDevices().First();
    
    using (SyncService service = new SyncService(new AdbSocket(), device))
    using (Stream stream = File.OpenRead(@"C:\MyFile.txt"))
    {
        service.Push(stream, "/data/MyFile.txt", null, CancellationToken.None);     
    }
}
```

### Run shell commands
To run shell commands on an Android device, you can use the `AdbClient.Instance.ExecuteRemoteCommand` method.

You need to pass a `DeviceData` object which specifies the device on which you want to run your command. You
can get a `DeviceData` object by calling `AdbClient.Instance.GetDevices()`, which will run one `DeviceData`
object for each device Android connected to your PC.

You'll also need to pass an `IOutputReceiver` object. Output receivers are classes that receive and parse the data
the device sends back. In this example, we'll use the standard `ConsoleOutputReceiver`, which reads all console
output and allows you to retrieve it as a single string. You can also use other output receivers or create your own.

```
void EchoTest()
{
    var device = AdbClient.Instance.GetDevices().First();
    var receiver = new ConsoleOutputReceiver();

    AdbClient.Instance.ExecuteRemoteCommand("echo Hello, World", device, receiver);

    Console.WriteLine("The device responded:");
    Console.WriteLine(receiver.ToString());
}
```

## History
SharpAdbClient is a fork of [madb](https://github.com/camalot/madb); which in itself is a .NET port of the 
[ddmlib Java Library](https://android.googlesource.com/platform/tools/base/+/master/ddmlib/). Credits for porting 
this library go to [Ryan Conrad](https://github.com/camalot).

