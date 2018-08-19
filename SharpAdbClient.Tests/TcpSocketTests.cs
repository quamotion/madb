using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class TcpSocketTests
    {
        [Fact]
        public void LifecycleTest()
        {
            TcpSocket socket = new TcpSocket();
            Assert.False(socket.Connected);

            socket.Connect(new DnsEndPoint("www.google.com", 80));
            Assert.True(socket.Connected);

            byte[] data = Encoding.ASCII.GetBytes(@"GET / HTTP/1.1

");
            socket.Send(data, 0, data.Length, SocketFlags.None);

            byte[] responseData = new byte[128];
            socket.Receive(responseData, 0, SocketFlags.None);

            string response = Encoding.ASCII.GetString(responseData);
            socket.Dispose();
        }

        [Fact]
        public void ReconnectTest()
        {
            TcpSocket socket = new TcpSocket();
            Assert.False(socket.Connected);

            socket.Connect(new DnsEndPoint("www.google.com", 80));
            Assert.True(socket.Connected);

            socket.Dispose();
            Assert.False(socket.Connected);

            socket.Reconnect();
            Assert.True(socket.Connected);
        }

        [Fact]
        public void BufferSizeTest()
        {
            TcpSocket socket = new TcpSocket();
            socket.ReceiveBufferSize = 1024;
            Assert.Equal(1024, socket.ReceiveBufferSize);
            socket.Dispose();
        }

        [Fact]
        public void CreateUnsupportedSocketTest()
        {
            TcpSocket socket = new TcpSocket();
            Assert.Throws<NotSupportedException>(() => socket.Connect(new CustomEndPoint()));
        }
    }
}
