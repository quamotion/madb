using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbServerTests
    {
        private DummyAdbSocket socket;
        private DummyAdbCommandLineClient commandLineClient;

        [TestInitialize]
        public void Initialize()
        {
            Factories.Reset();

            this.socket = new DummyAdbSocket();
            Factories.AdbSocketFactory = (endPoint) => this.socket;

            this.commandLineClient = new DummyAdbCommandLineClient();
            Factories.AdbCommandLineClientFactory = (version) => this.commandLineClient;
        }

        [TestMethod]
        public void GetStatusNotRunningTest()
        {
            Factories.AdbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            var status = AdbServer.Instance.GetStatus();
            Assert.IsFalse(status.IsRunning);
            Assert.IsNull(status.Version);
        }

        [TestMethod]
        public void GetStatusRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0020");

            var status = AdbServer.Instance.GetStatus();

            Assert.AreEqual(0, this.socket.Responses.Count);
            Assert.AreEqual(0, this.socket.ResponseMessages.Count);
            Assert.AreEqual(1, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);

            Assert.IsTrue(status.IsRunning);
            Assert.AreEqual(new Version(1, 0, 32), status.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public void GetStatusOtherSocketExceptionTest()
        {
            Factories.AdbSocketFactory = (endPoint) =>
            {
                throw new SocketException();
            };

            var status = AdbServer.Instance.GetStatus();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetStatusOtherExceptionTest()
        {
            Factories.AdbSocketFactory = (endPoint) =>
            {
                throw new Exception();
            };

            var status = AdbServer.Instance.GetStatus();
        }

        [TestMethod]
        public void StartServerAlreadyRunningTest()
        {
            this.commandLineClient.Version = new Version(1, 0, 20);
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0020");

            var result = AdbServer.Instance.StartServer(null, false);

            Assert.AreEqual(StartServerResult.AlreadyRunning, result);

            Assert.AreEqual(1, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
        public void StartServerOutdatedRunningNoExecutableTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0010");

            var result = AdbServer.Instance.StartServer(null, false);

            Assert.AreEqual(1, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
        public void StartServerNotRunningNoExecutableTest()
        {
            Factories.AdbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            var result = AdbServer.Instance.StartServer(null, false);
        }

        [TestMethod]
        public void StartServerOutdatedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0010");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            var result = AdbServer.Instance.StartServer("adb.exe", false);

            Assert.IsTrue(this.commandLineClient.ServerStarted);

            Assert.AreEqual(2, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
            Assert.AreEqual("host:kill", this.socket.Requests[1]);
        }

        [TestMethod]
        public void StartServerNotRunningTest()
        {
            Factories.AdbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            var result = AdbServer.Instance.StartServer("adb.exe", false);

            Assert.IsTrue(this.commandLineClient.ServerStarted);
        }

        [TestMethod]
        public void StartServerIntermediateRestartRequestedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("001f");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            var result = AdbServer.Instance.StartServer("adb.exe", true);

            Assert.IsTrue(this.commandLineClient.ServerStarted);

            Assert.AreEqual(2, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
            Assert.AreEqual("host:kill", this.socket.Requests[1]);
        }

        [TestMethod]
        public void StartServerIntermediateRestartNotRequestedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("001f");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            var result = AdbServer.Instance.StartServer("adb.exe", false);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            Assert.AreEqual(1, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var adbServer = new AdbServer();
            Assert.IsNotNull(adbServer);
            Assert.IsNotNull(adbServer.EndPoint);
            Assert.IsInstanceOfType(adbServer.EndPoint, typeof(IPEndPoint));

            var endPoint = (IPEndPoint)adbServer.EndPoint;

            Assert.AreEqual(IPAddress.Loopback, endPoint.Address);
            Assert.AreEqual(AdbServer.AdbServerPort, endPoint.Port);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConstructorInvalidEndPointTest()
        {
            var adbServer = new AdbServer(new CustomEndPoint());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEndPointTest()
        {
            var adbServer = new AdbServer(null);
        }
    }
}
