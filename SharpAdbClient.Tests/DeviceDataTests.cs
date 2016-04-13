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

            Assert.AreEqual("xyz", d.Serial);
        }

        [TestMethod]
        public void GetStateFromStringTest()
        {
            Assert.AreEqual(DeviceState.NoPermissions, DeviceData.GetStateFromString("no permissions"));
            Assert.AreEqual(DeviceState.Unknown, DeviceData.GetStateFromString("hello"));
        }
    }
}
