using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpAdbClient.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbServerTests
    {
        private Func<string, IAdbCommandLineClient> adbCommandLineClientFactory;
        private DummyAdbSocket socket;
        private DummyAdbCommandLineClient commandLineClient;
        private Func<EndPoint, IAdbSocket> adbSocketFactory;
        private AdbClient adbClient;
        private AdbServer adbServer;

        [TestInitialize]
        public void Initialize()
        {
            this.socket = new DummyAdbSocket();
            this.adbSocketFactory = (endPoint) => this.socket;

            this.commandLineClient = new DummyAdbCommandLineClient();
            this.adbCommandLineClientFactory = (version) => this.commandLineClient;

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);
        }

        [TestMethod]
        public void GetStatusNotRunningTest()
        {
            var adbClientMock = new Mock<IAdbClient>();
            adbClientMock.Setup(c => c.GetAdbVersion())
                .Throws(new SocketException(AdbServer.ConnectionRefused));

            var adbServer = new AdbServer(adbClientMock.Object, this.adbCommandLineClientFactory);

            var status = adbServer.GetStatus();
            Assert.IsFalse(status.IsRunning);
            Assert.IsNull(status.Version);
        }

        [TestMethod]
        public void GetStatusRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0020");

            var status = this.adbServer.GetStatus();

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
            this.adbSocketFactory = (endPoint) =>
            {
                throw new SocketException();
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            var status = this.adbServer.GetStatus();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetStatusOtherExceptionTest()
        {
            this.adbSocketFactory = (endPoint) =>
            {
                throw new Exception();
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            var status = this.adbServer.GetStatus();
        }

        [TestMethod]
        public void StartServerAlreadyRunningTest()
        {
            this.commandLineClient.Version = new Version(1, 0, 20);
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0020");

            var result = this.adbServer.StartServer(null, false);

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

            var result = this.adbServer.StartServer(null, false);

            Assert.AreEqual(1, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
        public void StartServerNotRunningNoExecutableTest()
        {
            this.adbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            var result = this.adbServer.StartServer(null, false);
        }

        [TestMethod]
        public void StartServerOutdatedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0010");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            var result = this.adbServer.StartServer("adb.exe", false);

            Assert.IsTrue(this.commandLineClient.ServerStarted);

            Assert.AreEqual(2, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
            Assert.AreEqual("host:kill", this.socket.Requests[1]);
        }

        [TestMethod]
        public void StartServerNotRunningTest()
        {
            this.adbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            var result = this.adbServer.StartServer("adb.exe", false);

            Assert.IsTrue(this.commandLineClient.ServerStarted);
        }

        [TestMethod]
        public void StartServerIntermediateRestartRequestedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("001f");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            var result = this.adbServer.StartServer("adb.exe", true);

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

            var result = this.adbServer.StartServer("adb.exe", false);

            Assert.IsFalse(this.commandLineClient.ServerStarted);

            Assert.AreEqual(1, this.socket.Requests.Count);
            Assert.AreEqual("host:version", this.socket.Requests[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorAdbClientNullTest()
        {
            var adbServer = new AdbServer(null, this.adbCommandLineClientFactory);
        }
    }
}
