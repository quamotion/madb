using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class AdbHelperTests
    {
        [TestMethod]
        public void FormAdbRequestTest()
        {
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0009host:kill\n"), AdbHelper.FormAdbRequest("host:kill"));
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("000Chost:version\n"), AdbHelper.FormAdbRequest("host:version"));
        }

        [TestMethod]
        public void CreateAdbForwardRequestTest()
        {
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0008tcp:1984\n"), AdbHelper.CreateAdbForwardRequest(null, 1984));
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0012tcp:1981:127.0.0.1\n"), AdbHelper.CreateAdbForwardRequest("127.0.0.1", 1981));
        }

        DummyAdbSocketFactory factory;
        DummyAdbSocket socket;
        IPEndPoint endPoint;

        [TestInitialize]
        public void Initialize()
        {
            this.factory = new DummyAdbSocketFactory();
            this.socket = factory.Socket;
            AdbHelper.SocketFactory = factory;
            this.endPoint = new IPEndPoint(IPAddress.Loopback, 1);
        }

        [TestMethod]
        public void GetAdbVersionTest()
        {
            socket.Responses.Enqueue(new AdbResponse());
            socket.ResponseMessages.Enqueue("000f");
            var version = AdbHelper.Instance.GetAdbVersion(endPoint);

            // Make sure the messages were read
            Assert.AreEqual(0, socket.ResponseMessages.Count);
            Assert.AreEqual(0, socket.Responses.Count);

            // Make sure a request was sent
            Assert.AreEqual(1, socket.Requests.Count);
            Assert.AreEqual("host:version", socket.Requests[0]);

            // ... and the correct value is returned.
            Assert.AreEqual(15, version);
        }
    }
}
