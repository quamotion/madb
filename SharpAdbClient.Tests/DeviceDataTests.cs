using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class DeviceDataTests
    {
       
        [TestMethod]
        public void CreateFromDeviceDataTest()
        {
            string data = "99000000               device product:if_s200n model:NL_V100KR device:if_s200n";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.AreEqual<string>("99000000", device.Serial);
            Assert.AreEqual<DeviceState>(DeviceState.Online, device.State);
            Assert.AreEqual<string>("if_s200n", device.Product);
            Assert.AreEqual<string>("NL_V100KR", device.Model);
            Assert.AreEqual<string>("if_s200n", device.Name);
        }
        
        [TestMethod]
        public void ToStringTest()
        {
            DeviceData d = new DeviceData();
            d.Serial = "xyz";

            Assert.AreEqual("xyz", d.ToString());
        }

        [TestMethod]
        public void GetStateFromStringTest()
        {
            Assert.AreEqual(DeviceState.NoPermissions, DeviceData.GetStateFromString("no permissions"));
            Assert.AreEqual(DeviceState.Unknown, DeviceData.GetStateFromString("hello"));
        }
        
        [TestMethod]
        public void CreateFromDeviceDataTransportIdTest()
        {
            string data = "emulator-5554          device product:sdk_google_phone_x86 model:Android_SDK_built_for_x86 device:generic_x86 transport_id:1";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.AreEqual<string>("emulator-5554", device.Serial);
            Assert.AreEqual<string>("sdk_google_phone_x86", device.Product);
            Assert.AreEqual<string>("Android_SDK_built_for_x86", device.Model);
            Assert.AreEqual<string>("generic_x86", device.Name);
            Assert.AreEqual<DeviceState>(DeviceState.Online, device.State);
        }
    }
}
