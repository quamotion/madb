using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Logs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        [DeploymentItem("logcat.bin")]
        public async Task ReadLogTests()
        {
            using (Stream stream = File.OpenRead(@"logcat.bin"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                LogReader reader = new LogReader(shellStream);

                // This stream contains 3 log entries. Read & validate the first one,
                // read the next two ones (to be sure all data is read correctly).
                var log = await reader.ReadEntry(CancellationToken.None);

                Assert.IsInstanceOfType(log, typeof(AndroidLogEntry));

                Assert.AreEqual(707, log.ProcessId);
                Assert.AreEqual(1254, log.ThreadId);
                Assert.AreEqual(3u, log.Id);
                Assert.IsNotNull(log.Data);
                Assert.AreEqual(179, log.Data.Length);
                Assert.AreEqual(new DateTime(2015, 11, 14, 23, 38, 20, 590, DateTimeKind.Utc), log.TimeStamp);

                var androidLog = (AndroidLogEntry)log;
                Assert.AreEqual(Priority.Info, androidLog.Priority);
                Assert.AreEqual("ActivityManager", androidLog.Tag);
                Assert.AreEqual("Start proc com.google.android.gm for broadcast com.google.android.gm/.widget.GmailWidgetProvider: pid=7026 uid=10066 gids={50066, 9997, 3003, 1028, 1015} abi=x86", androidLog.Message);

                Assert.IsNotNull(await reader.ReadEntry(CancellationToken.None));
                Assert.IsNotNull(await reader.ReadEntry(CancellationToken.None));
            }
        }

        [TestMethod]
        [DeploymentItem("logcatevents.bin")]
        public async Task ReadEventLogTest()
        {
            // The data in this stream was read using a ShellStream, so the CRLF fixing
            // has already taken place.
            using (Stream stream = File.OpenRead(@"logcatevents.bin"))
            {
                LogReader reader = new LogReader(stream);
                var entry = await reader.ReadEntry(CancellationToken.None);

                Assert.IsInstanceOfType(entry, typeof(EventLogEntry));
                Assert.AreEqual(707, entry.ProcessId);
                Assert.AreEqual(1291, entry.ThreadId);
                Assert.AreEqual(2u, entry.Id);
                Assert.IsNotNull(entry.Data);
                Assert.AreEqual(39, entry.Data.Length);
                Assert.AreEqual(new DateTime(2015, 11, 16, 1, 48, 40, 525, DateTimeKind.Utc), entry.TimeStamp);

                var eventLog = (EventLogEntry)entry;
                Assert.AreEqual(0, eventLog.Tag);
                Assert.IsNotNull(eventLog.Values);
                Assert.AreEqual(1, eventLog.Values.Count);
                Assert.IsNotNull(eventLog.Values[0]);
                Assert.IsInstanceOfType(eventLog.Values[0], typeof(Collection<object>));

                var list = (Collection<object>)eventLog.Values[0];
                Assert.AreEqual(3, list.Count);
                Assert.AreEqual(0, list[0]);
                Assert.AreEqual(19512, list[1]);
                Assert.AreEqual("com.amazon.kindle", list[2]);

                entry = await reader.ReadEntry(CancellationToken.None);
                entry = await reader.ReadEntry(CancellationToken.None);
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task ReadLogEntryTest()
        {
            var device = AdbClient.Instance.GetDevices().Single();
            await AdbClient.Instance.RunLogServiceAsync(device, (logEntry) => System.Diagnostics.Debug.WriteLine(logEntry.ToString()), CancellationToken.None, LogId.System);
        }
    }
}
