using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class DeviceMonitorTests : SocketBasedTests
    {
        [TestInitialize]
        public void Initialize()
        {
            // Toggle the integration test flag to true to run on an actual adb server
            // (and to build/validate the test cases), set to false to use the mocked
            // adb sockets.
            // In release mode, this flag is ignored and the mocked adb sockets are always used.
            base.Initialize(integrationTest: false, doDispose: true);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            using (DeviceMonitor monitor = new DeviceMonitor(this.Socket))
            {
                Assert.IsNotNull(monitor.Devices);
                Assert.AreEqual(0, monitor.Devices.Count);
                Assert.AreEqual(this.Socket, monitor.Socket);
                Assert.IsFalse(monitor.IsRunning);
            }
        }

        [TestMethod]
        public void StartInitialDeviceListTest()
        {
            using (DeviceMonitor monitor = new DeviceMonitor(this.Socket))
            {
                Assert.AreEqual(0, monitor.Devices.Count);

                base.RunTest(
                OkResponse,
                ResponseMessages("169.254.109.177:5555\tdevice\n"),
                Requests("host:track-devices"),
                () =>
                {
                    monitor.Start();

                    Assert.AreEqual(1, monitor.Devices.Count);
                    Assert.AreEqual("169.254.109.177:5555", monitor.Devices.ElementAt(0).Serial);
                });
            }
        }
    }
}
