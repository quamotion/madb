using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class PackageManagerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest()
        {
            new PackageManager(null);
        }

        [TestMethod]
        public void PackagesPropertyTest()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            AdbClient.Instance = client;
            PackageManager manager = new PackageManager(device);

            Assert.IsTrue(manager.Packages.ContainsKey("com.android.gallery3d"));
            Assert.AreEqual("/system/app/Gallery2/Gallery2.apk", manager.Packages["com.android.gallery3d"]);
        }

        [TestMethod]
        public void PackagesPropertyTest2()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("pm list packages -f", "package:mwc2015.be");
            AdbClient.Instance = client;
            PackageManager manager = new PackageManager(device);

            Assert.IsTrue(manager.Packages.ContainsKey("mwc2015.be"));
            Assert.AreEqual(null, manager.Packages["mwc2015.be"]);
        }

        [TestMethod]
        public void InstallRemotePackageTest()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            adbClient.Commands.Add("pm install /data/test.apk", string.Empty);
            adbClient.Commands.Add("pm install -r /data/test.apk", string.Empty);

            AdbClient.Instance = adbClient;

            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new PackageManager(device);
            manager.InstallRemotePackage("/data/test.apk", false);

            Assert.AreEqual(2, adbClient.ReceivedCommands.Count);
            Assert.AreEqual("pm install /data/test.apk", adbClient.ReceivedCommands[1]);

            manager.InstallRemotePackage("/data/test.apk", true);

            Assert.AreEqual(3, adbClient.ReceivedCommands.Count);
            Assert.AreEqual("pm install -r /data/test.apk", adbClient.ReceivedCommands[2]);
        }

        [TestMethod]
        [DeploymentItem("test.txt")]
        public void InstallPackageTest()
        {
            var syncService = new DummySyncService();
            Factories.SyncServiceFactory = (d) => syncService;

            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            adbClient.Commands.Add("pm install /data/local/tmp/test.txt", string.Empty);
            adbClient.Commands.Add("rm /data/local/tmp/test.txt", string.Empty);

            AdbClient.Instance = adbClient;

            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new PackageManager(device);
            manager.InstallPackage("test.txt", false);
            Assert.AreEqual(3, adbClient.ReceivedCommands.Count);
            Assert.AreEqual("pm install /data/local/tmp/test.txt", adbClient.ReceivedCommands[1]);
            Assert.AreEqual("rm /data/local/tmp/test.txt", adbClient.ReceivedCommands[2]);

            Assert.AreEqual(1, syncService.UploadedFiles.Count);
            Assert.IsTrue(syncService.UploadedFiles.ContainsKey("/data/local/tmp/test.txt"));
        }

        [TestMethod]
        public void UninstallPackageTest()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            client.Commands.Add("pm uninstall com.android.gallery3d", "Success");
            AdbClient.Instance = client;
            PackageManager manager = new PackageManager(device);

            // Command should execute correctly; if the wrong command is passed an exception
            // would be thrown.
            manager.UninstallPackage("com.android.gallery3d");
        }
    }
}
