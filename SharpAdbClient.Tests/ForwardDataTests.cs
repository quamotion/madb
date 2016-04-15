using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class ForwardDataTests
    {
        [TestMethod]
        public void SpecTests()
        {
            ForwardData data = new ForwardData();
            data.Local = "tcp:1234";
            data.Remote = "tcp:4321";

            Assert.AreEqual("tcp:1234", data.LocalSpec.ToString());
            Assert.AreEqual("tcp:4321", data.RemoteSpec.ToString());
        }
    }
}
