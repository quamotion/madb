using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpAdbClient.DeviceCommands;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class DeviceExtensionsTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Factories.Reset();
        }

        [TestMethod]
        public void StatTest()
        {
            FileStatistics stats = new FileStatistics();

            var mock = new Mock<ISyncService>();
            mock.Setup(m => m.Stat("/test")).Returns(stats);

            Factories.SyncServiceFactory = (d) => mock.Object;

            var device = new DeviceData();

            Assert.AreEqual(stats, device.Stat("/test"));
        }

        [TestMethod]
        public void GetEnvironmentVariablesTest()
        {
            var adbClient = new DummyAdbClient();
            AdbClient.Instance = adbClient;

            adbClient.Commands.Add(EnvironmentVariablesReceiver.PrintEnvCommand, "a=b");

            var device = new DeviceData();

            var variables = device.GetEnvironmentVariables();
            Assert.IsNotNull(variables);
            Assert.AreEqual(1, variables.Keys.Count);
            Assert.IsTrue(variables.ContainsKey("a"));
            Assert.AreEqual("b", variables["a"]);
        }
    }
}
