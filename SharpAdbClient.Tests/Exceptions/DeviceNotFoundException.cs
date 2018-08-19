using Xunit;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    public class DeviceNotFoundExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor()
        {
            ExceptionTester<DeviceNotFoundException>.TestEmptyConstructor(() => new DeviceNotFoundException());
        }

        [Fact]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<DeviceNotFoundException>.TestMessageAndInnerConstructor((message, inner) => new DeviceNotFoundException(message, inner));
        }

#if !NETCOREAPP1_1
        [Fact]
        public void TestSerializationConstructor()
        {
            ExceptionTester<DeviceNotFoundException>.TestSerializationConstructor((info, context) => new DeviceNotFoundException(info, context));
        }
#endif
    }
}
