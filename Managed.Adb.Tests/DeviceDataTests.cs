using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Managed.Adb.Tests
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
    }
}
