using Xunit;
using SharpAdbClient.Logs;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    public class ShellStreamTests
    {
        [Fact]
        public void ConstructorNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new ShellStream(null, false));
        }

        [Fact]
        public void ConstructorWriteOnlyTest()
        {
            var temp = Path.GetTempFileName();

            try
            {
                using (FileStream stream = File.OpenWrite(temp))
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => new ShellStream(stream, false));
                }
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [Fact]
        public void ConstructorTest()
        {
            using (MemoryStream stream = new MemoryStream())
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                Assert.Equal(stream, shellStream.Inner);
                Assert.True(shellStream.CanRead);
                Assert.False(shellStream.CanSeek);
                Assert.False(shellStream.CanWrite);
            }
        }

        [Fact]
        public void TestCRLFAtStart()
        {
            using (MemoryStream stream = GetStream("\r\nHello, World!"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            using (StreamReader reader = new StreamReader(shellStream))
            {
                Assert.Equal((int)'\n', shellStream.ReadByte());

                stream.Position = 0;
                byte[] buffer = new byte[2];
                var read = shellStream.Read(buffer, 0, 2);
                Assert.Equal(2, read);
                Assert.Equal((byte)'\n', buffer[0]);
                Assert.Equal((byte)'H', buffer[1]);

                stream.Position = 0;
                Assert.Equal("\nHello, World!", reader.ReadToEnd());
            }
        }

        [Fact]
        public void MultipleCRLFInString()
        {
            using (MemoryStream stream = GetStream("\r\n1\r\n2\r\n3\r\n4\r\n5"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            using (StreamReader reader = new StreamReader(shellStream))
            {
                Assert.Equal((int)'\n', shellStream.ReadByte());

                stream.Position = 0;
                byte[] buffer = new byte[100];
                var read = shellStream.Read(buffer, 0, 100);

                var actual = Encoding.ASCII.GetString(buffer, 0, read);
                Assert.Equal("\n1\n2\n3\n4\n5", actual);
                Assert.Equal(10, read);

                for (int i = 10; i < buffer.Length; i++)
                {
                    Assert.Equal(0, buffer[i]);
                }
            }
        }

        [Fact]
        public void PendingByteTest1()
        {
            using (MemoryStream stream = GetStream("\r\nH\ra"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                byte[] buffer = new byte[1];
                var read = shellStream.Read(buffer, 0, 1);
                Assert.Equal(1, read);
                Assert.Equal((byte)'\n', buffer[0]);

                read = shellStream.Read(buffer, 0, 1);
                Assert.Equal(1, read);
                Assert.Equal((byte)'H', buffer[0]);

                read = shellStream.Read(buffer, 0, 1);
                Assert.Equal(1, read);
                Assert.Equal((byte)'\r', buffer[0]);

                read = shellStream.Read(buffer, 0, 1);
                Assert.Equal(1, read);
                Assert.Equal((byte)'a', buffer[0]);
            }
        }

        [Fact]
        public async Task TestCRLFAtStartAsync()
        {
            using (MemoryStream stream = GetStream("\r\nHello, World!"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            using (StreamReader reader = new StreamReader(shellStream))
            {
                Assert.Equal((int)'\n', shellStream.ReadByte());

                stream.Position = 0;
                byte[] buffer = new byte[2];
                var read = await shellStream.ReadAsync(buffer, 0, 2).ConfigureAwait(false);
                Assert.Equal(2, read);
                Assert.Equal((byte)'\n', buffer[0]);
                Assert.Equal((byte)'H', buffer[1]);

                stream.Position = 0;
                Assert.Equal("\nHello, World!", reader.ReadToEnd());
            }
        }

        [Fact]
        public async Task MultipleCRLFInStringAsync()
        {
            using (MemoryStream stream = GetStream("\r\n1\r\n2\r\n3\r\n4\r\n5"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            using (StreamReader reader = new StreamReader(shellStream))
            {
                Assert.Equal((int)'\n', shellStream.ReadByte());

                stream.Position = 0;
                byte[] buffer = new byte[100];
                var read = await shellStream.ReadAsync(buffer, 0, 100).ConfigureAwait(false);

                var actual = Encoding.ASCII.GetString(buffer, 0, read);
                Assert.Equal("\n1\n2\n3\n4\n5", actual);
                Assert.Equal(10, read);

                for (int i = 10; i < buffer.Length; i++)
                {
                    Assert.Equal(0, buffer[i]);
                }
            }
        }

        [Fact]
        public async Task PendingByteTest1Async()
        {
            using (MemoryStream stream = GetStream("\r\nH\ra"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                byte[] buffer = new byte[1];
                var read = await shellStream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                Assert.Equal(1, read);
                Assert.Equal((byte)'\n', buffer[0]);

                read = await shellStream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                Assert.Equal(1, read);
                Assert.Equal((byte)'H', buffer[0]);

                read = await shellStream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                Assert.Equal(1, read);
                Assert.Equal((byte)'\r', buffer[0]);

                read = await shellStream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                Assert.Equal(1, read);
                Assert.Equal((byte)'a', buffer[0]);
            }
        }

        private MemoryStream GetStream(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            return new MemoryStream(data);
        }
    }
}
