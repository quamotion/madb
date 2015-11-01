using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class AdbServerTests
    {
        private DummyAdbSocket socket;
        private DummyAdbSocketFactory socketFactory;
        private DummyAdbCommandLineClient commandLineClient;

        [TestInitialize]
        public void Initialize()
        {
            this.socketFactory = new DummyAdbSocketFactory();
            this.socket = this.socketFactory.Socket;

            this.commandLineClient = new DummyAdbCommandLineClient();
            AdbHelper.SocketFactory = this.socketFactory;
            AdbServer.AdbCommandLineClientFactory = (version) => this.commandLineClient;
        }

        [TestMethod]
        public void GetStatusNotRunningTest()
        {
            this.socketFactory.Exception = new SocketException(AdbServer.ConnectionRefused);

            var status = AdbServer.GetStatus();
            Assert.IsFalse(status.IsRunning);
            Assert.IsNull(status.Version);
        }

        [TestMethod]
        public void GetStatusRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0020");

            var status = AdbServer.GetStatus();

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
            this.socketFactory.Exception = new SocketException();

            var status = AdbServer.GetStatus();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetStatusOtherExceptionTest()
        {
            this.socketFactory.Exception = new Exception();

            var status = AdbServer.GetStatus();
        }
    }
}
