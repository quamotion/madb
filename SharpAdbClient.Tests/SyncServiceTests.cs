using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SharpAdbClient.Tests
{
    public class SyncServiceTests : SocketBasedTests
    {
        // Toggle the integration test flag to true to run on an actual adb server
        // (and to build/validate the test cases), set to false to use the mocked
        // adb sockets.
        // In release mode, this flag is ignored and the mocked adb sockets are always used.
        public SyncServiceTests()
            : base(integrationTest: false, doDispose: false)
        {
        }

        [Fact]
        public void StatTest()
        {
            DeviceData device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            FileStatistics value = null;

            this.RunTest(
                OkResponses(2),
                NoResponseMessages,
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.STAT, "/fstab.donatello"),
                new SyncCommand[] { SyncCommand.STAT },
                new byte[][] { new byte[] { 160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0 } },
                null,
                () =>
                {
                    using (SyncService service = new SyncService(this.Socket, device))
                    {
                        value = service.Stat("/fstab.donatello");
                    }
                });

            Assert.NotNull(value);
            Assert.Equal(UnixFileMode.Regular, value.FileMode & UnixFileMode.TypeMask);
            Assert.Equal(597, value.Size);
            Assert.Equal(DateTimeHelper.Epoch.ToLocalTime(), value.Time);
        }

        [Fact]
        public void GetListingTest()
        {
            DeviceData device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            List<FileStatistics> value = null;

            this.RunTest(
                OkResponses(2),
                ResponseMessages(".", "..", "sdcard0", "emulated"),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.LIST, "/storage"),
                new SyncCommand[] { SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DONE },
                new byte[][]
                {
                    new byte[] { 233, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86 },
                    new byte[] { 237, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86 },
                    new byte[] { 255, 161, 0, 0, 24, 0, 0, 0, 152, 130, 56, 86 },
                    new byte[] { 109, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86 }
                },
                null,
                () =>
                {
                    using (SyncService service = new SyncService(this.Socket, device))
                    {
                        value = service.GetDirectoryListing("/storage").ToList();
                    }
                });

            Assert.Equal(4, value.Count);

            var time = new DateTime(2015, 11, 3, 9, 47, 4, DateTimeKind.Utc).ToLocalTime();

            var dir = value[0];
            Assert.Equal(".", dir.Path);
            Assert.Equal((UnixFileMode)16873, dir.FileMode);
            Assert.Equal(0, dir.Size);
            Assert.Equal(time, dir.Time);

            var parentDir = value[1];
            Assert.Equal("..", parentDir.Path);
            Assert.Equal((UnixFileMode)16877, parentDir.FileMode);
            Assert.Equal(0, parentDir.Size);
            Assert.Equal(time, parentDir.Time);

            var sdcard0 = value[2];
            Assert.Equal("sdcard0", sdcard0.Path);
            Assert.Equal((UnixFileMode)41471, sdcard0.FileMode);
            Assert.Equal(24, sdcard0.Size);
            Assert.Equal(time, sdcard0.Time);

            var emulated = value[3];
            Assert.Equal("emulated", emulated.Path);
            Assert.Equal((UnixFileMode)16749, emulated.FileMode);
            Assert.Equal(0, emulated.Size);
            Assert.Equal(time, emulated.Time);
        }

        [Fact]
        public void PullTest()
        {
            DeviceData device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            MemoryStream stream = new MemoryStream();
            var content = File.ReadAllBytes("fstab.bin");
            var contentLength = BitConverter.GetBytes(content.Length);

            this.RunTest(
                OkResponses(2),
                ResponseMessages(),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.STAT, "/fstab.donatello").Union(SyncRequests(SyncCommand.RECV, "/fstab.donatello")),
                new SyncCommand[] { SyncCommand.STAT, SyncCommand.DATA, SyncCommand.DONE },
                new byte[][]
                {
                    new byte[] { 160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0 },
                    contentLength,
                    content
                },
                null,
                () =>
                {
                    using (SyncService service = new SyncService(this.Socket, device))
                    {
                        service.Pull("/fstab.donatello", stream, null, CancellationToken.None);
                    }
                });

            // Make sure the data that has been sent to the stream is the expected data
            Assert.Equal(content, stream.ToArray());
        }

        [Fact]
        public void PushTest()
        {
            DeviceData device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            Stream stream = File.OpenRead("fstab.bin");
            var content = File.ReadAllBytes("fstab.bin");
            var contentMessage = new List<byte>();
            contentMessage.AddRange(SyncCommandConverter.GetBytes(SyncCommand.DATA));
            contentMessage.AddRange(BitConverter.GetBytes(content.Length));
            contentMessage.AddRange(content);

            this.RunTest(
                OkResponses(2),
                ResponseMessages(),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(
                    SyncCommand.SEND, "/sdcard/test,644",
                    SyncCommand.DONE, "1446505200"),
                new SyncCommand[] { SyncCommand.OKAY },
                null,
                new byte[][]
                {
                    contentMessage.ToArray()
                },
                () =>
                {
                    using (SyncService service = new SyncService(this.Socket, device))
                    {
                        service.Push(stream, "/sdcard/test", 0644, new DateTime(2015, 11, 2, 23, 0, 0, DateTimeKind.Utc), null, CancellationToken.None);
                    }
                });
        }
    }
}
