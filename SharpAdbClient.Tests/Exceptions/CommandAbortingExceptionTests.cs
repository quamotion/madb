using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    [TestClass]
    public class CommandAbortingExceptionTests
    {
        [TestMethod]
        public void TestEmptyConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestEmptyConstructor(() => new CommandAbortingException());
        }

        [TestMethod]
        public void TestMessageConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestMessageConstructor((message) => new CommandAbortingException(message));
        }

        [TestMethod]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestMessageAndInnerConstructor((message, inner) => new CommandAbortingException(message, inner));
        }

#if !NETCOREAPP1_1
        [TestMethod]
        public void TestSerializationConstructor()
        {
            ExceptionTester<CommandAbortingException>.TestSerializationConstructor((info, context) => new CommandAbortingException(info, context));
        }
#endif
    }
}
