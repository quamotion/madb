using Xunit;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    public class UnknownOptionExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestEmptyConstructor(() => new UnknownOptionException());
        }

        [Fact]
        public void TestMessageConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestMessageConstructor((message) => new UnknownOptionException(message));
        }

        [Fact]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestMessageAndInnerConstructor((message, inner) => new UnknownOptionException(message, inner));
        }

#if !NETCOREAPP1_1
        [Fact]
        public void TestSerializationConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestSerializationConstructor((info, context) => new UnknownOptionException(info, context));
        }
#endif
    }
}
