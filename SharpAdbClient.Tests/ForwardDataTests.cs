using Xunit;

namespace SharpAdbClient.Tests
{
    public class ForwardDataTests
    {
        [Fact]
        public void SpecTests()
        {
            ForwardData data = new ForwardData();
            data.Local = "tcp:1234";
            data.Remote = "tcp:4321";

            Assert.Equal("tcp:1234", data.LocalSpec.ToString());
            Assert.Equal("tcp:4321", data.RemoteSpec.ToString());
        }
    }
}
