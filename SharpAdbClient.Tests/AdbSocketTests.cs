using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;
using SharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbSocketTests
    {
        [TestMethod]
        public void CloseTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            Assert.IsTrue(socket.Connected);

            socket.Dispose();
            Assert.IsFalse(socket.Connected);
        }

        [TestMethod]
        public void DisposeTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            Assert.IsTrue(socket.Connected);

            socket.Dispose();
            Assert.IsFalse(socket.Connected);
        }

        [TestMethod]
        public void IsOkayTest()
        {
            var okay = Encoding.ASCII.GetBytes("OKAY");
            var fail = Encoding.ASCII.GetBytes("FAIL");

            Assert.IsTrue(AdbSocket.IsOkay(okay));
            Assert.IsFalse(AdbSocket.IsOkay(fail));
        }

        [TestMethod]
        public void SendSyncRequestTest()
        {
            this.RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.DATA, 2),
                new byte[] { (byte)'D', (byte)'A', (byte)'T', (byte)'A', 2, 0, 0, 0 });
        }

        [TestMethod]
        public void SendSyncRequestTest2()
        {
            this.RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.SEND, "/test"),
                new byte[] { (byte)'S', (byte)'E', (byte)'N', (byte)'D', 5, 0, 0, 0, (byte)'/', (byte)'t', (byte)'e', (byte)'s', (byte)'t' });
        }

        [TestMethod]
        public void SendSyncRequest3()
        {
            this.RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.DENT, "/data", 633),
                new byte[] { (byte)'D', (byte)'E', (byte)'N', (byte)'T', 9, 0, 0, 0, (byte)'/', (byte)'d', (byte)'a', (byte)'t', (byte)'a', (byte)',', (byte)'6', (byte)'3', (byte)'3' });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendSyncNullRequestTest()
        {
            this.RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.DATA, null),
                new byte[] { });
        }

        [TestMethod]
        public void ReadSyncResponse()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            using (StreamWriter writer = new StreamWriter(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                writer.Write("DENT");
            }

            tcpSocket.InputStream.Position = 0;

            Assert.AreEqual(SyncCommand.DENT, socket.ReadSyncResponse());
        }

        [TestMethod]
        public void ReadSyncString()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            using (BinaryWriter writer = new BinaryWriter(tcpSocket.InputStream, Encoding.ASCII, true))
            {
                writer.Write(5);
                writer.Write(Encoding.ASCII.GetBytes("Hello"));
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.AreEqual("Hello", socket.ReadSyncString());
        }

        [TestMethod]
        public async Task ReadStringAsyncTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            using (BinaryWriter writer = new BinaryWriter(tcpSocket.InputStream, Encoding.ASCII, true))
            {
                writer.Write(Encoding.ASCII.GetBytes(5.ToString("X4")));
                writer.Write(Encoding.ASCII.GetBytes("Hello"));
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.AreEqual("Hello", await socket.ReadStringAsync(CancellationToken.None));
        }

        [TestMethod]
        public void ReadAdbOkayResponseTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            using (StreamWriter writer = new StreamWriter(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                writer.Write("OKAY");
            }

            tcpSocket.InputStream.Position = 0;

            var response = socket.ReadAdbResponse();
            Assert.IsTrue(response.IOSuccess);
            Assert.AreEqual(string.Empty, response.Message);
            Assert.IsTrue(response.Okay);
            Assert.IsFalse(response.Timeout);
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
        public void ReadAdbFailResponseTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            using (StreamWriter writer = new StreamWriter(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                writer.Write("FAIL");
                writer.Write(17.ToString("X4"));
                writer.Write("This did not work");
            }

            tcpSocket.InputStream.Position = 0;

            var response = socket.ReadAdbResponse();
        }

        [TestMethod]
        public void ReadTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            tcpSocket.InputStream.Write(data, 0, 101);
            tcpSocket.InputStream.Position = 0;

            // Buffer has a capacity of 101, but we'll only want to read 100 bytes
            byte[] received = new byte[101];

            socket.Read(received, 100);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(received[i], (byte)i);
            }

            Assert.AreEqual(0, received[100]);
        }

        [TestMethod]
        public async Task ReadAsyncTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            tcpSocket.InputStream.Write(data, 0, 101);
            tcpSocket.InputStream.Position = 0;

            // Buffer has a capacity of 101, but we'll only want to read 100 bytes
            byte[] received = new byte[101];

            await socket.ReadAsync(received, 100, CancellationToken.None);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(received[i], (byte)i);
            }

            Assert.AreEqual(0, received[100]);
        }

        [TestMethod]
        public void SendAdbRequestTest()
        {
            this.RunTest(
                (socket) => socket.SendAdbRequest("Test"),
                Encoding.ASCII.GetBytes("0004Test\n"));
        }

        [TestMethod]
        public void GetShellStreamTest()
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            var stream = socket.GetShellStream();
            Assert.IsInstanceOfType(stream, typeof(ShellStream));

            var shellStream = (ShellStream)stream;
            Assert.AreEqual(tcpSocket.OutputStream, shellStream.Inner);
        }

        private void RunTest(Action<IAdbSocket> test, byte[] expectedDataSent)
        {
            DummyTcpSocket tcpSocket = new DummyTcpSocket();
            AdbSocket socket = new AdbSocket(tcpSocket);

            // Run the test.
            test(socket);

            // Validate the data that was sent over the wire.
            CollectionAssert.AreEqual(expectedDataSent, tcpSocket.GetBytesSent());
        }
    }
}
