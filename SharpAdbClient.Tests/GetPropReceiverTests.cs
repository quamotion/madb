using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class GetPropReceiverTests
    {
        [TestMethod]
        public void ListPropertiesTest()
        {
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();
            client.Commands.Add("/system/bin/getprop", @"[init.svc.BGW]: [running]
[init.svc.MtkCodecService]: [running]
[init.svc.bootanim]: [stopped]");
            AdbClient.Instance = client;

            var properties = device.GetProperties();
            Assert.IsNotNull(properties);
            Assert.AreEqual(3, properties.Count);
            Assert.IsTrue(properties.ContainsKey("init.svc.BGW"));
            Assert.IsTrue(properties.ContainsKey("init.svc.MtkCodecService"));
            Assert.IsTrue(properties.ContainsKey("init.svc.bootanim"));

            Assert.AreEqual("running", properties["init.svc.BGW"]);
            Assert.AreEqual("running", properties["init.svc.MtkCodecService"]);
            Assert.AreEqual("stopped", properties["init.svc.bootanim"]);
        }
    }
}
