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

        [TestMethod]
        public void EqualsTest()
        {
            var dummy = new ForwardSpec()
            {
                Protocol = (ForwardProtocol)99
            };

            var tcpa1 = ForwardSpec.Parse("tcp:1234");
            var tcpa2 = ForwardSpec.Parse("tcp:1234");
            var tcpb = ForwardSpec.Parse("tcp:4321");
            Assert.IsTrue(tcpa1.Equals(tcpa2));
            Assert.IsFalse(tcpa1.Equals(tcpb));
            Assert.IsTrue(tcpa1.GetHashCode() == tcpa2.GetHashCode());
            Assert.IsFalse(tcpa1.GetHashCode() == tcpb.GetHashCode());

            var jdwpa1 = ForwardSpec.Parse("jdwp:1234");
            var jdwpa2 = ForwardSpec.Parse("jdwp:1234");
            var jdwpb = ForwardSpec.Parse("jdwp:4321");
            Assert.IsTrue(jdwpa1.Equals(jdwpa2));
            Assert.IsFalse(jdwpa1.Equals(jdwpb));
            Assert.IsTrue(jdwpa1.GetHashCode() == jdwpa2.GetHashCode());
            Assert.IsFalse(jdwpa1.GetHashCode() == jdwpb.GetHashCode());

            var socketa1 = ForwardSpec.Parse("localabstract:/tmp/1234");
            var socketa2 = ForwardSpec.Parse("localabstract:/tmp/1234");
            var socketb = ForwardSpec.Parse("localabstract:/tmp/4321");
            Assert.IsTrue(socketa1.Equals(socketa2));
            Assert.IsFalse(socketa1.Equals(socketb));
            Assert.IsTrue(socketa1.GetHashCode() == socketa2.GetHashCode());
            Assert.IsFalse(socketa1.GetHashCode() == socketb.GetHashCode());

            Assert.IsFalse(tcpa1.Equals(null));
            Assert.IsFalse(tcpa1.Equals(dummy));
            Assert.IsFalse(dummy.Equals(tcpa1));
            Assert.IsFalse(tcpa1.Equals(jdwpa1));
            Assert.IsFalse(tcpa1.Equals(socketa1));
            Assert.IsFalse(tcpa1.GetHashCode() == dummy.GetHashCode());
            Assert.IsFalse(tcpa1.GetHashCode() == jdwpa1.GetHashCode());
            Assert.IsFalse(tcpa1.GetHashCode() == socketa1.GetHashCode());
        }
    }
}
