using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    [TestClass]
    public class AdbExceptionTests
    {
        [TestMethod]
        public void TestEmptyConstructor()
        {
            ExceptionTester<AdbException>.TestEmptyConstructor(() => new AdbException());
        }

        [TestMethod]
        public void TestMessageConstructor()
        {
            ExceptionTester<AdbException>.TestMessageConstructor((message) => new AdbException(message));
        }

        [TestMethod]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<AdbException>.TestMessageAndInnerConstructor((message, inner) => new AdbException(message, inner));
        }

        [TestMethod]
        public void TestSerializationConstructor()
        {
            ExceptionTester<AdbException>.TestSerializationConstructor((info, context) => new AdbException(info, context));
        }
    }
}
