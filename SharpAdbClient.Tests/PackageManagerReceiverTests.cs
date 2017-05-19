using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class PackageManagerReceiverTests
    {
        [TestMethod]
        public void ParseThirdPartyPackage()
        {
            // Arrange
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            AdbClient.Instance = client;

            PackageManager manager = new PackageManager(device, thirdPartyOnly: false, client: client, syncServiceFactory: null, skipInit: true);
            PackageManagerReceiver receiver = new PackageManagerReceiver(device, manager);

            // Act
            receiver.AddOutput("package:/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk=com.google.android.apps.plus");
            receiver.AddOutput("package:/system/app/LegacyCamera.apk=com.android.camera");
            receiver.AddOutput("package:mwc2015.be");
            receiver.Flush();

            // Assert
            Assert.AreEqual(3, manager.Packages.Count);
            Assert.IsTrue(manager.Packages.ContainsKey("com.google.android.apps.plus"));
            Assert.IsTrue(manager.Packages.ContainsKey("com.android.camera"));
            Assert.IsTrue(manager.Packages.ContainsKey("mwc2015.be"));

            Assert.AreEqual("/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk", manager.Packages["com.google.android.apps.plus"]);
        }
    }
}
