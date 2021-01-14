using Xunit;
using SharpAdbClient.DeviceCommands;
using System;
using System.IO;
using Moq;

namespace SharpAdbClient.Tests
{
    public class PackageManagerTests
    {
        [Fact]
        public void ConstructorNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new PackageManager(null, null));
            Assert.Throws<ArgumentNullException>(() => new PackageManager(null, new DeviceData()));
            Assert.Throws<ArgumentNullException>(() => new PackageManager(Mock.Of<IAdbClient>(), null));
        }

        [Fact]
        public void PackagesPropertyTest()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            PackageManager manager = new PackageManager(client, device);

            Assert.True(manager.Packages.ContainsKey("com.android.gallery3d"));
            Assert.Equal("/system/app/Gallery2/Gallery2.apk", manager.Packages["com.android.gallery3d"]);
        }

        [Fact]
        public void PackagesPropertyTest2()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("pm list packages -f", "package:mwc2015.be");
            PackageManager manager = new PackageManager(client, device);

            Assert.True(manager.Packages.ContainsKey("mwc2015.be"));
            Assert.Null(manager.Packages["mwc2015.be"]);
        }

        [Fact]
        public void InstallRemotePackageTest()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            adbClient.Commands.Add("pm install /data/test.apk", string.Empty);
            adbClient.Commands.Add("pm install -r /data/test.apk", string.Empty);

            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new PackageManager(adbClient, device);
            manager.InstallRemotePackage("/data/test.apk", false);

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install /data/test.apk", adbClient.ReceivedCommands[1]);

            manager.InstallRemotePackage("/data/test.apk", true);

            Assert.Equal(3, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install -r /data/test.apk", adbClient.ReceivedCommands[2]);
        }

        [Fact]
        public void InstallPackageTest()
        {
            var syncService = new DummySyncService();
            Factories.SyncServiceFactory = (c, d) => syncService;

            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            adbClient.Commands.Add("pm install /data/local/tmp/test.txt", string.Empty);
            adbClient.Commands.Add("rm /data/local/tmp/test.txt", string.Empty);

            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new PackageManager(adbClient, device);
            manager.InstallPackage("test.txt", false);
            Assert.Equal(3, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install /data/local/tmp/test.txt", adbClient.ReceivedCommands[1]);
            Assert.Equal("rm /data/local/tmp/test.txt", adbClient.ReceivedCommands[2]);

            Assert.Single(syncService.UploadedFiles);
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/test.txt"));
        }

        [Fact]
        public void UninstallPackageTest()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            client.Commands.Add("pm uninstall com.android.gallery3d", "Success");
            PackageManager manager = new PackageManager(client, device);

            // Command should execute correctly; if the wrong command is passed an exception
            // would be thrown.
            manager.UninstallPackage("com.android.gallery3d");
        }

        [Fact]
        public void GetPackageVersionInfoTest()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("dumpsys package com.google.android.gms", File.ReadAllText("gapps.txt"));
            PackageManager manager = new PackageManager(client, device, skipInit: true);

            var versionInfo = manager.GetVersionInfo("com.google.android.gms");
            Assert.Equal(11062448, versionInfo.VersionCode);
            Assert.Equal("11.0.62 (448-160311229)", versionInfo.VersionName);
        }
    }
}
