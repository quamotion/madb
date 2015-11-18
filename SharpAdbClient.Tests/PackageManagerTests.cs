using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpAdbClient.DeviceCommands;
using System.Collections.Generic;
using System.Threading;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class PackageManagerTests
    {
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
