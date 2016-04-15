using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class ForwardSpecTests
    {
        [TestMethod]
        public void TcpTest()
        {
            var value = ForwardSpec.Parse("tcp:1234");

            Assert.IsNotNull(value);
            Assert.AreEqual(ForwardProtocol.Tcp, value.Protocol);
            Assert.AreEqual(1234, value.Port);
            Assert.AreEqual(0, value.ProcessId);
            Assert.IsNull(value.SocketName);

            Assert.AreEqual("tcp:1234", value.ToString());
        }

        [TestMethod]
        public void SocketText()
        {
            var value = ForwardSpec.Parse("localabstract:/tmp/1234");

            Assert.IsNotNull(value);
            Assert.AreEqual(ForwardProtocol.LocalAbstract, value.Protocol);
            Assert.AreEqual(0, value.Port);
            Assert.AreEqual(0, value.ProcessId);
            Assert.AreEqual("/tmp/1234", value.SocketName);

            Assert.AreEqual("localabstract:/tmp/1234", value.ToString());
        }

        [TestMethod]
        public void JdwpTest()
        {
            var value = ForwardSpec.Parse("jdwp:1234");

            Assert.IsNotNull(value);
            Assert.AreEqual(ForwardProtocol.JavaDebugWireProtocol, value.Protocol);
            Assert.AreEqual(0, value.Port);
            Assert.AreEqual(1234, value.ProcessId);
            Assert.IsNull(value.SocketName);

            Assert.AreEqual("jdwp:1234", value.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParseNullTest()
        {
            ForwardSpec.Parse(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ParseInvalidTest()
        {
            ForwardSpec.Parse("abc");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ParseInvalidTcpPortTest()
        {
            ForwardSpec.Parse("tcp:xyz");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ParseInvalidProcessIdTest()
        {
            ForwardSpec.Parse("jdwp:abc");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ParseInvalidProtocolTest()
        {
            ForwardSpec.Parse("xyz:1234");
        }

        [TestMethod]
        public void ToStringInvalidProtocol()
        {
            var spec = new ForwardSpec();
            spec.Protocol = (ForwardProtocol)99;
            Assert.AreEqual(string.Empty, spec.ToString());
        }
    }
}
