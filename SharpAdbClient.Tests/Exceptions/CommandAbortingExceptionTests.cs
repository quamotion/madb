using Xunit;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    public class CommandAbortingExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestEmptyConstructor(() => new CommandAbortingException());
        }

        [Fact]
        public void TestMessageConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestMessageConstructor((message) => new CommandAbortingException(message));
        }

        [Fact]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestMessageAndInnerConstructor((message, inner) => new CommandAbortingException(message, inner));
        }

#if !NETCOREAPP1_1
        [Fact]
        public void TestSerializationConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestSerializationConstructor((info, context) => new CommandAbortingException(info, context));
        }
#endif
    }
}
