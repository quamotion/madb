using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpAdbClient.DeviceCommands;
using System.Linq;

namespace SharpAdbClient.Tests.DeviceCommands
{
    [TestClass]
    public class DeviceExtensionsTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Factories.Reset();
        }

        [TestMethod]
        public void StatTest()
        {
            FileStatistics stats = new FileStatistics();

            var mock = new Mock<ISyncService>();
            mock.Setup(m => m.Stat("/test")).Returns(stats);

            Factories.SyncServiceFactory = (d) => mock.Object;

            var device = new DeviceData();

            Assert.AreEqual(stats, device.Stat("/test"));
        }

        [TestMethod]
        public void GetEnvironmentVariablesTest()
        {
            var adbClient = new DummyAdbClient();
            AdbClient.Instance = adbClient;

            adbClient.Commands.Add(EnvironmentVariablesReceiver.PrintEnvCommand, "a=b");

            var device = new DeviceData();

            var variables = device.GetEnvironmentVariables();
            Assert.IsNotNull(variables);
            Assert.AreEqual(1, variables.Keys.Count);
            Assert.IsTrue(variables.ContainsKey("a"));
            Assert.AreEqual("b", variables["a"]);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void ListProcessesIntegrationTest()
        {
            var device = AdbClient.Instance.GetDevices().Single();
            var processes = device.ListProcesses();
        }

        [TestMethod]
        public void UninstallPackageTests()
        {
            var adbClient = new DummyAdbClient();
            AdbClient.Instance = adbClient;

            adbClient.Commands.Add("pm list packages -f", "");
            adbClient.Commands.Add("pm uninstall com.example", "");

            var device = new DeviceData();
            device.State = DeviceState.Online;
            device.UninstallPackage("com.example");

            Assert.AreEqual(2, adbClient.ReceivedCommands.Count);
            Assert.AreEqual("pm list packages -f", adbClient.ReceivedCommands[0]);
            Assert.AreEqual("pm uninstall com.example", adbClient.ReceivedCommands[1]);
        }

        [TestMethod]
        public void GetPackageVersionTest()
        {
            var adbClient = new DummyAdbClient();
            AdbClient.Instance = adbClient;

            adbClient.Commands.Add("pm list packages -f", "");
            adbClient.Commands.Add("dumpsys package com.example",
@"Activity Resolver Table:
  Non-Data Actions:
      com.android.providers.contacts.DUMP_DATABASE:
        310a0bd8 com.android.providers.contacts/.debug.ContactsDumpActivity

Receiver Resolver Table:
  Schemes:
      package:
        31f30b31 com.android.providers.contacts/.PackageIntentReceiver (4 filters)

Registered ContentProviders:
  com.android.providers.contacts/.debug.DumpFileProvider:
    Provider{2b000d84 com.android.providers.contacts/.debug.DumpFileProvider}

ContentProvider Authorities:
  [com.android.voicemail]:
    Provider{316ea633 com.android.providers.contacts/.VoicemailContentProvider}
      applicationInfo=ApplicationInfo{1327df0 com.android.providers.contacts}

Key Set Manager:
  [com.android.providers.contacts]
      Signing KeySets: 3

Packages:
  Package [com.android.providers.contacts] (3d5205d5):
    versionCode=22 targetSdk=22
    versionName=5.1-eng.buildbot.20151117.204057
    splits=[base]

Shared users:
  SharedUser [android.uid.shared] (3341dee):
    userId=10002 gids=[3003, 1028, 1015]
    grantedPermissions:
      android.permission.WRITE_SETTINGS");

            var device = new DeviceData();
            device.State = DeviceState.Online;
            var version = device.GetPackageVersion("com.example");

            Assert.AreEqual(22, version.VersionCode);
            Assert.AreEqual("5.1-eng.buildbot.20151117.204057", version.VersionName);

            Assert.AreEqual(2, adbClient.ReceivedCommands.Count);
            Assert.AreEqual("pm list packages -f", adbClient.ReceivedCommands[0]);
            Assert.AreEqual("dumpsys package com.example", adbClient.ReceivedCommands[1]);
        }

        [TestMethod]
        public void ListProcessesTest()
        {
            var adbClient = new DummyAdbClient();
            AdbClient.Instance = adbClient;

            adbClient.Commands.Add("/system/bin/ls /proc/", 
@"1
2
3
acpi
asound");
            adbClient.Commands.Add("cat /proc/1/stat /proc/2/stat /proc/3/stat ",
@"1 (init) S 0 0 0 0 -1 1077944576 2680 83280 0 179 0 67 16 39 20 0 1 0 2 17735680 143 18446744073709551615 134512640 135145076 4288071392 4288070744 134658736 0 0 0 65536 18446744071580117077 0 0 17 1 0 0 0 0 0 135152736 135165080 142131200 4288073690 4288073696 4288073696 4288073714 0
2 (kthreadd) S 0 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 2 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579254310 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
3 (ksoftirqd/0) S 2 0 0 0 -1 69238848 0 0 0 0 0 23 0 0 20 0 1 0 7 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579284070 0 0 17 0 0 0 0 0 0 0 0 0 0 0 0 0 0");


            var device = new DeviceData();
            var processes = device.ListProcesses().ToArray();

            Assert.AreEqual(3, processes.Length);
            Assert.AreEqual("init", processes[0].Name);
            Assert.AreEqual("kthreadd", processes[1].Name);
            Assert.AreEqual("ksoftirqd/0", processes[2].Name);
        }
    }
}
