using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests {
    [TestClass]
	public class DeviceTests : BaseDeviceTests {

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void BackupTest ( ) {
			var device = GetFirstDevice ( );
			device.Backup ( );
		}

        [TestMethod]
        public void CreateFromDeviceDataVSEmulatorTest()
        {
            string data = @"169.254.138.177:5555   offline product:VS Emulator Android Device - 480 x 800 model:Android_Device___480_x_800 device:donatello";

            var device = Device.CreateFromAdbData(data);
            Assert.AreEqual<string>("169.254.138.177:5555", device.SerialNumber);
            Assert.AreEqual<string>("VS Emulator Android Device - 480 x 800", device.Product);
            Assert.AreEqual<string>("Android_Device___480_x_800", device.Model);
            Assert.AreEqual<string>("donatello", device.DeviceProperty);
            Assert.AreEqual<DeviceState>(DeviceState.Offline, device.State);
        }

        [TestMethod]
        public void CreateFromDeviceDataUnauthorizedTest()
        {
            string data = "R32D102SZAE            unauthorized";

            var device = Device.CreateFromAdbData(data);
            Assert.AreEqual<string>("R32D102SZAE", device.SerialNumber);
            Assert.AreEqual<string>("", device.Product);
            Assert.AreEqual<string>("", device.Model);
            Assert.AreEqual<string>("", device.DeviceProperty);
            Assert.AreEqual<DeviceState>(DeviceState.Unauthorized, device.State);
        }
	}
}
