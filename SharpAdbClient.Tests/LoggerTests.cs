using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Logs;
using System;
using System.IO;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        [DeploymentItem("logcat.bin")]
        public void ReadLogTests()
        {
            using (Stream stream = File.OpenRead(@"logcat.bin"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            using (LogReader reader = new LogReader(shellStream))
            {
                // This stream contains 3 log entries. Read & validate the first one,
                // read the next two ones (to be sure all data is read correctly).
                var log = reader.ReadEntry();

                Assert.AreEqual(707, log.ProcessId);
                Assert.AreEqual(1254, log.ThreadId);
                Assert.AreEqual(3u, log.Id);
                Assert.IsNotNull(log.Data);
                Assert.AreEqual(179, log.Data.Length);
                Assert.AreEqual(new DateTime(2015, 11, 14, 23, 38, 20, 590, DateTimeKind.Utc), log.TimeStamp);

                Assert.IsNotNull(reader.ReadEntry());
                Assert.IsNotNull(reader.ReadEntry());
            }
        }
    }
}
