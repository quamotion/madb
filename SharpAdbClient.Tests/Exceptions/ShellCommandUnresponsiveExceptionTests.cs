using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;
using SharpAdbClient.Tests;

namespace SharpAdbClient.Tests.Exceptions
{
    [TestClass]
    public class ShellCommandUnresponsiveExceptionTests
    {
        [TestMethod]
        public void TestEmptyConstructor()
        {
            ExceptionTester<ShellCommandUnresponsiveException>.TestEmptyConstructor(() => new ShellCommandUnresponsiveException());
        }

        [TestMethod]
        public void TestMessageConstructor()
        {
            ExceptionTester<ShellCommandUnresponsiveException>.TestMessageConstructor((message) => new ShellCommandUnresponsiveException(message));
        }

        [TestMethod]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<ShellCommandUnresponsiveException>.TestMessageAndInnerConstructor((message, inner) => new ShellCommandUnresponsiveException(message, inner));
        }

#if !NETCOREAPP1_1
        [TestMethod]
        public void TestSerializationConstructor()
        {
            ExceptionTester<ShellCommandUnresponsiveException>.TestSerializationConstructor((info, context) => new ShellCommandUnresponsiveException(info, context));
        }
#endif
    }
}
