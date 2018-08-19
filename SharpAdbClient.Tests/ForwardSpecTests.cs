using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    public class ForwardSpecTests
    {
        [Fact]
        public void TcpTest()
        {
            var value = ForwardSpec.Parse("tcp:1234");

            Assert.NotNull(value);
            Assert.Equal(ForwardProtocol.Tcp, value.Protocol);
            Assert.Equal(1234, value.Port);
            Assert.Equal(0, value.ProcessId);
            Assert.Null(value.SocketName);

            Assert.Equal("tcp:1234", value.ToString());
        }

        [Fact]
        public void SocketText()
        {
            var value = ForwardSpec.Parse("localabstract:/tmp/1234");

            Assert.NotNull(value);
            Assert.Equal(ForwardProtocol.LocalAbstract, value.Protocol);
            Assert.Equal(0, value.Port);
            Assert.Equal(0, value.ProcessId);
            Assert.Equal("/tmp/1234", value.SocketName);

            Assert.Equal("localabstract:/tmp/1234", value.ToString());
        }

        [Fact]
        public void JdwpTest()
        {
            var value = ForwardSpec.Parse("jdwp:1234");

            Assert.NotNull(value);
            Assert.Equal(ForwardProtocol.JavaDebugWireProtocol, value.Protocol);
            Assert.Equal(0, value.Port);
            Assert.Equal(1234, value.ProcessId);
            Assert.Null(value.SocketName);

            Assert.Equal("jdwp:1234", value.ToString());
        }

        [Fact]
        public void ParseNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ForwardSpec.Parse(null));
        }

        [Fact]
        public void ParseInvalidTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ForwardSpec.Parse("abc"));
        }

        [Fact]
        public void ParseInvalidTcpPortTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ForwardSpec.Parse("tcp:xyz"));
        }

        [Fact]
        public void ParseInvalidProcessIdTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ForwardSpec.Parse("jdwp:abc"));
        }

        [Fact]
        public void ParseInvalidProtocolTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ForwardSpec.Parse("xyz:1234"));
        }

        [Fact]
        public void ToStringInvalidProtocol()
        {
            var spec = new ForwardSpec();
            spec.Protocol = (ForwardProtocol)99;
            Assert.Equal(string.Empty, spec.ToString());
        }

        [Fact]
        public void EqualsTest()
        {
            var dummy = new ForwardSpec()
            {
                Protocol = (ForwardProtocol)99
            };

            var tcpa1 = ForwardSpec.Parse("tcp:1234");
            var tcpa2 = ForwardSpec.Parse("tcp:1234");
            var tcpb = ForwardSpec.Parse("tcp:4321");
            Assert.True(tcpa1.Equals(tcpa2));
            Assert.False(tcpa1.Equals(tcpb));
            Assert.True(tcpa1.GetHashCode() == tcpa2.GetHashCode());
            Assert.False(tcpa1.GetHashCode() == tcpb.GetHashCode());

            var jdwpa1 = ForwardSpec.Parse("jdwp:1234");
            var jdwpa2 = ForwardSpec.Parse("jdwp:1234");
            var jdwpb = ForwardSpec.Parse("jdwp:4321");
            Assert.True(jdwpa1.Equals(jdwpa2));
            Assert.False(jdwpa1.Equals(jdwpb));
            Assert.True(jdwpa1.GetHashCode() == jdwpa2.GetHashCode());
            Assert.False(jdwpa1.GetHashCode() == jdwpb.GetHashCode());

            var socketa1 = ForwardSpec.Parse("localabstract:/tmp/1234");
            var socketa2 = ForwardSpec.Parse("localabstract:/tmp/1234");
            var socketb = ForwardSpec.Parse("localabstract:/tmp/4321");
            Assert.True(socketa1.Equals(socketa2));
            Assert.False(socketa1.Equals(socketb));
            Assert.True(socketa1.GetHashCode() == socketa2.GetHashCode());
            Assert.False(socketa1.GetHashCode() == socketb.GetHashCode());

            Assert.False(tcpa1.Equals(null));
            Assert.False(tcpa1.Equals(dummy));
            Assert.False(dummy.Equals(tcpa1));
            Assert.False(tcpa1.Equals(jdwpa1));
            Assert.False(tcpa1.Equals(socketa1));
            Assert.False(tcpa1.GetHashCode() == dummy.GetHashCode());
            Assert.False(tcpa1.GetHashCode() == jdwpa1.GetHashCode());
            Assert.False(tcpa1.GetHashCode() == socketa1.GetHashCode());
        }
    }
}
