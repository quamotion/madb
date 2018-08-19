using Xunit;

namespace SharpAdbClient.Tests.Extensions
{
    public class ArrayHelperTests
    {
        [Fact]
        public void Swap32bitFromArrayTest()
        {
            byte[] value = new byte[] { 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };
            var number = value.Swap32BitFromArray(1);
            Assert.Equal(0x32547698, number);
        }

        [Fact]
        public void SwapU16bitFromArrayTest()
        {
            byte[] value = new byte[] { 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };
            var number = value.SwapU16BitFromArray(1);
            Assert.Equal(0x7698, number);
        }
    }
}
