using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class TcpSocketTests
    {
        [TestMethod]
        public void LifecycleTest()
        {
            TcpSocket socket = new TcpSocket();
            Assert.IsFalse(socket.Connected);

            socket.Connect(new DnsEndPoint("www.google.com", 80));
            Assert.IsTrue(socket.Connected);

            byte[] data = Encoding.ASCII.GetBytes(@"GET / HTTP/1.1

");
            socket.Send(data, 0, data.Length, SocketFlags.None);

            byte[] responseData = new byte[128];
            socket.Receive(responseData, 0, SocketFlags.None);

            string response = Encoding.ASCII.GetString(responseData);
            socket.Dispose();
        }

        [TestMethod]
        public void ReconnectTest()
        {
            TcpSocket socket = new TcpSocket();
            Assert.IsFalse(socket.Connected);

            socket.Connect(new DnsEndPoint("www.google.com", 80));
            Assert.IsTrue(socket.Connected);

            socket.Dispose();
            Assert.IsFalse(socket.Connected);

            socket.Reconnect();
            Assert.IsTrue(socket.Connected);
        }

        [TestMethod]
        public void BufferSizeTest()
        {
            TcpSocket socket = new TcpSocket();
            socket.ReceiveBufferSize = 1024;
            Assert.AreEqual(1024, socket.ReceiveBufferSize);
            socket.Dispose();
        }

        [TestMethod]
        public void CreateSocketTest()
        {
            var winSocket = TcpSocket.CreateSocket(new IPEndPoint(IPAddress.Loopback, 0));
            Assert.AreEqual(AddressFamily.InterNetwork, winSocket.AddressFamily);
            Assert.AreEqual(SocketType.Stream, winSocket.SocketType);
            Assert.AreEqual(ProtocolType.Tcp, winSocket.ProtocolType);

            // Unix sockets cannot be created on Windows machines
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CreateUnsupportedSocketTest()
        {
            TcpSocket.CreateSocket(new CustomEndPoint());
        }
    }
}
