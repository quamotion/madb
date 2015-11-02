using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class SyncServiceTests : SocketBasedTests
    {
        [TestInitialize]
        public void Initialize()
        {
            // Toggle the integration test flag to true to run on an actual adb server
            // (and to build/validate the test cases), set to false to use the mocked
            // adb sockets.
            // In release mode, this flag is ignored and the mocked adb sockets are always used.
            base.Initialize(integrationTest: true, doDispose: false);
        }

        [TestMethod]
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
                () =>
                {
                    using (SyncService service = new SyncService(this.Socket, device))
                    {
                        value = service.Stat("/fstab.donatello");
                    }
                });

            Assert.IsNotNull(value);
            Assert.AreEqual(UnixFileMode.Regular, value.FileMode & UnixFileMode.TypeMask);
            Assert.AreEqual(597, value.Size);
            Assert.AreEqual(ManagedAdbExtenstions.Epoch.ToLocalTime(), value.Time);
        }

        [TestMethod]
        public void GetListingTest()
        {
            DeviceData device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            SyncService service = new SyncService(device);
            var entries = service.GetDirectoryListing("/storage");
        }
    }
}
