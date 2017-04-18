using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SharpAdbClient.Tests
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest()
        {
            DeviceMonitor monitor = new DeviceMonitor(null);
        }

        [TestMethod]
        public void DeviceDisconnectedTest()
        {
            this.Socket.WaitForNewData = true;

            using (DeviceMonitor monitor = new DeviceMonitor(this.Socket))
            {
                DeviceMonitorSink sink = new DeviceMonitorSink(monitor);

                Assert.AreEqual(0, monitor.Devices.Count);

                // Start the monitor, detect the initial device.
                base.RunTest(
                OkResponse,
                ResponseMessages("169.254.109.177:5555\tdevice\n"),
                Requests("host:track-devices"),
                () =>
                {
                    monitor.Start();

                    Assert.AreEqual(1, monitor.Devices.Count);
                    Assert.AreEqual(1, sink.ConnectedEvents.Count);
                    Assert.AreEqual(0, sink.ChangedEvents.Count);
                    Assert.AreEqual(0, sink.DisconnectedEvents.Count);
                });

                this.Socket.ResponseMessages.Clear();
                this.Socket.Responses.Clear();
                this.Socket.Requests.Clear();

                // Device disconnects
                var eventWaiter = sink.CreateEventSignal();

                base.RunTest(
                NoResponses,
                ResponseMessages(""),
                Requests(),
                () =>
                {
                    eventWaiter.WaitOne(1000);
                    Assert.AreEqual(0, monitor.Devices.Count);
                    Assert.AreEqual(1, sink.ConnectedEvents.Count);
                    Assert.AreEqual(0, sink.ChangedEvents.Count);
                    Assert.AreEqual(1, sink.DisconnectedEvents.Count);
                    Assert.AreEqual("169.254.109.177:5555", sink.DisconnectedEvents[0].Device.Serial);
                });
            }
        }

        [TestMethod]
        public void DeviceConnectedTest()
        {
            this.Socket.WaitForNewData = true;

            using (DeviceMonitor monitor = new DeviceMonitor(this.Socket))
            {
                DeviceMonitorSink sink = new DeviceMonitorSink(monitor);

                Assert.AreEqual(0, monitor.Devices.Count);

                // Start the monitor, detect the initial device.
                base.RunTest(
                OkResponse,
                ResponseMessages(""),
                Requests("host:track-devices"),
                () =>
                {
                    monitor.Start();

                    Assert.AreEqual(0, monitor.Devices.Count);
                    Assert.AreEqual(0, sink.ConnectedEvents.Count);
                    Assert.AreEqual(0, sink.ChangedEvents.Count);
                    Assert.AreEqual(0, sink.DisconnectedEvents.Count);
                });

                this.Socket.ResponseMessages.Clear();
                this.Socket.Responses.Clear();
                this.Socket.Requests.Clear();

                // Device disconnects
                var eventWaiter = sink.CreateEventSignal();

                base.RunTest(
                NoResponses,
                ResponseMessages("169.254.109.177:5555\tdevice\n"),
                Requests(),
                () =>
                {
                    eventWaiter.WaitOne(1000);

                    Assert.AreEqual(1, monitor.Devices.Count);
                    Assert.AreEqual(1, sink.ConnectedEvents.Count);
                    Assert.AreEqual(0, sink.ChangedEvents.Count);
                    Assert.AreEqual(0, sink.DisconnectedEvents.Count);
                    Assert.AreEqual("169.254.109.177:5555", sink.ConnectedEvents[0].Device.Serial);
                });
            }
        }

        [TestMethod]
        public void StartInitialDeviceListTest()
        {
            this.Socket.WaitForNewData = true;

            using (DeviceMonitor monitor = new DeviceMonitor(this.Socket))
            {
                DeviceMonitorSink sink = new DeviceMonitorSink(monitor);

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
                    Assert.AreEqual(1, sink.ConnectedEvents.Count);
                    Assert.AreEqual("169.254.109.177:5555", sink.ConnectedEvents[0].Device.Serial);
                    Assert.AreEqual(0, sink.ChangedEvents.Count);
                    Assert.AreEqual(0, sink.DisconnectedEvents.Count);
                });
            }
        }

        [TestMethod]
        public void DeviceChangedTest()
        {
            this.Socket.WaitForNewData = true;

            using (DeviceMonitor monitor = new DeviceMonitor(this.Socket))
            {
                DeviceMonitorSink sink = new DeviceMonitorSink(monitor);

                Assert.AreEqual(0, monitor.Devices.Count);

                // Start the monitor, detect the initial device.
                base.RunTest(
                OkResponse,
                ResponseMessages("169.254.109.177:5555\toffline\n"),
                Requests("host:track-devices"),
                () =>
                {
                    monitor.Start();

                    Assert.AreEqual(1, monitor.Devices.Count);
                    Assert.AreEqual(DeviceState.Offline, monitor.Devices.ElementAt(0).State);
                    Assert.AreEqual(1, sink.ConnectedEvents.Count);
                    Assert.AreEqual(0, sink.ChangedEvents.Count);
                    Assert.AreEqual(0, sink.DisconnectedEvents.Count);
                });

                this.Socket.ResponseMessages.Clear();
                this.Socket.Responses.Clear();
                this.Socket.Requests.Clear();

                // Device disconnects
                var eventWaiter = sink.CreateEventSignal();

                base.RunTest(
                NoResponses,
                ResponseMessages("169.254.109.177:5555\tdevice\n"),
                Requests(),
                () =>
                {
                    eventWaiter.WaitOne(1000);

                    Assert.AreEqual(1, monitor.Devices.Count);
                    Assert.AreEqual(DeviceState.Online, monitor.Devices.ElementAt(0).State);
                    Assert.AreEqual(1, sink.ConnectedEvents.Count);
                    Assert.AreEqual(1, sink.ChangedEvents.Count);
                    Assert.AreEqual(0, sink.DisconnectedEvents.Count);
                    Assert.AreEqual("169.254.109.177:5555", sink.ChangedEvents[0].Device.Serial);
                });
            }
        }

        /// <summary>
        /// Tests the <see cref="DeviceMonitor"/> in a case where the adb server dies in the middle of the monitor
        /// loop. The <see cref="DeviceMonitor"/> should detect this condition and restart the adb server.
        /// </summary>
        [TestMethod]
        public void AdbKilledTest()
        {
            var dummyAdbServer = new DummyAdbServer();
            AdbServer.Instance = dummyAdbServer;

            this.Socket.WaitForNewData = true;

            using (DeviceMonitor monitor = new DeviceMonitor(this.Socket))
            {
                base.RunTest(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                ResponseMessages(
                    DummyAdbSocket.ServerDisconnected,
                    string.Empty),
                Requests(
                    "host:track-devices",
                    "host:track-devices"),
                () =>
                {
                    monitor.Start();

                    Assert.IsTrue(this.Socket.DidReconnect);
                    Assert.IsTrue(dummyAdbServer.WasRestarted);
                });
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DisposeMonitorTest()
        {
            using (DeviceMonitor monitor = new DeviceMonitor(new AdbSocket(AdbClient.Instance.EndPoint)))
            {
                monitor.Start();
                Thread.Sleep(1000);
            }
        }
    }
}
