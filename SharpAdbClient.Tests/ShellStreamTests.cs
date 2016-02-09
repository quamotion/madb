using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class ShellStreamTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest()
        {
            new ShellStream(null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorWriteOnlyTest()
        {
            var temp = Path.GetTempFileName();

            try
            {
                using (FileStream stream = File.OpenWrite(temp))
                {
                    ShellStream shellStream = new ShellStream(stream, false);
                }
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [TestMethod]
        public void ConstructorTest()
        {
            using (MemoryStream stream = new MemoryStream())
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                Assert.AreEqual(stream, shellStream.Inner);
                Assert.IsTrue(shellStream.CanRead);
                Assert.IsFalse(shellStream.CanSeek);
                Assert.IsFalse(shellStream.CanWrite);
            }
        }

        [TestMethod]
        public void TestCRLFAtStart()
        {
            using (MemoryStream stream = GetStream("\r\nHello, World!"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            using (StreamReader reader = new StreamReader(shellStream))
            {
                Assert.AreEqual((int)'\n', shellStream.ReadByte());

                stream.Position = 0;
                byte[] buffer = new byte[2];
                var read = shellStream.Read(buffer, 0, 2);
                Assert.AreEqual(2, read);
                Assert.AreEqual((byte)'\n', buffer[0]);
                Assert.AreEqual((byte)'H', buffer[1]);

                stream.Position = 0;
                Assert.AreEqual("\nHello, World!", reader.ReadToEnd());
            }
        }

        [TestMethod]
        public void MultipleCRLFInString()
        {
            using (MemoryStream stream = GetStream("\r\n1\r\n2\r\n3\r\n4\r\n5"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            using (StreamReader reader = new StreamReader(shellStream))
            {
                Assert.AreEqual((int)'\n', shellStream.ReadByte());

                stream.Position = 0;
                byte[] buffer = new byte[100];
                var read = shellStream.Read(buffer, 0, 100);

                var actual = Encoding.ASCII.GetString(buffer, 0, read);
                Assert.AreEqual(actual, "\n1\n2\n3\n4\n5");
                Assert.AreEqual(10, read);

                for (int i = 10; i < buffer.Length; i++)
                {
                    Assert.AreEqual(0, buffer[i]);
                }
            }
        }

        [TestMethod]
        public void PendingByteTest1()
        {
            using (MemoryStream stream = GetStream("\r\nH\ra"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                byte[] buffer = new byte[1];
                var read = shellStream.Read(buffer, 0, 1);
                Assert.AreEqual(1, read);
                Assert.AreEqual((byte)'\n', buffer[0]);

                read = shellStream.Read(buffer, 0, 1);
                Assert.AreEqual(1, read);
                Assert.AreEqual((byte)'H', buffer[0]);

                read = shellStream.Read(buffer, 0, 1);
                Assert.AreEqual(1, read);
                Assert.AreEqual((byte)'\r', buffer[0]);

                read = shellStream.Read(buffer, 0, 1);
                Assert.AreEqual(1, read);
                Assert.AreEqual((byte)'a', buffer[0]);
            }
        }

        private MemoryStream GetStream(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            return new MemoryStream(data);
        }
    }
}
