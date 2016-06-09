using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class DeviceDataTests
    {
        [TestMethod]
        public void CreateFromDeviceDataVSEmulatorTest()
        {
            string data = @"169.254.138.177:5555   offline product:VS Emulator Android Device - 480 x 800 model:Android_Device___480_x_800 device:donatello";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.AreEqual<string>("169.254.138.177:5555", device.Serial);
            Assert.AreEqual<string>("VS Emulator Android Device - 480 x 800", device.Product);
            Assert.AreEqual<string>("Android_Device___480_x_800", device.Model);
            Assert.AreEqual<string>("donatello", device.Name);
            Assert.AreEqual<DeviceState>(DeviceState.Offline, device.State);
        }

        [TestMethod]
        public void CreateFromDeviceDataUnauthorizedTest()
        {
            string data = "R32D102SZAE            unauthorized";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.AreEqual<string>("R32D102SZAE", device.Serial);
            Assert.AreEqual<string>("", device.Product);
            Assert.AreEqual<string>("", device.Model);
            Assert.AreEqual<string>("", device.Name);
            Assert.AreEqual<DeviceState>(DeviceState.Unauthorized, device.State);
        }

        [TestMethod]
        public void CreateFromEmulatorTest()
        {
            string data = "emulator-5586          host features:shell_2";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.AreEqual<string>("emulator-5586", device.Serial);
            Assert.AreEqual<DeviceState>(DeviceState.Host, device.State);
            Assert.AreEqual("shell_2", device.Features);
        }

        [TestMethod]
        public void CreateWithFeaturesTest()
        {
            string data = "0100a9ee51a18f2b device product:bullhead model:Nexus_5X device:bullhead features:shell_v2,cmd";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.AreEqual<string>("0100a9ee51a18f2b", device.Serial);
            Assert.AreEqual<DeviceState>(DeviceState.Online, device.State);
            Assert.AreEqual("Nexus_5X", device.Model);
            Assert.AreEqual("bullhead", device.Product);
            Assert.AreEqual("bullhead", device.Name);
            Assert.AreEqual("shell_v2,cmd", device.Features);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateFromInvalidDatatest()
        {
            string data = "xyz";

            var device = DeviceData.CreateFromAdbData(data);
        }

        [TestMethod]
        public void ToStringTest()
        {
            DeviceData d = new DeviceData();
            d.Serial = "xyz";

            Assert.AreEqual("xyz", d.ToString());
        }

        [TestMethod]
        public void GetStateFromStringTest()
        {
            Assert.AreEqual(DeviceState.NoPermissions, DeviceData.GetStateFromString("no permissions"));
            Assert.AreEqual(DeviceState.Unknown, DeviceData.GetStateFromString("hello"));
        }
    }
}
