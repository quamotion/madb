using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    [TestClass]
    public class UnknownOptionExceptionTests
    {
        [TestMethod]
        public void TestEmptyConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestEmptyConstructor(() => new UnknownOptionException());
        }

        [TestMethod]
        public void TestMessageConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestMessageConstructor((message) => new UnknownOptionException(message));
        }

        [TestMethod]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestMessageAndInnerConstructor((message, inner) => new UnknownOptionException(message, inner));
        }

        [TestMethod]
        public void TestSerializationConstructor()
        {
            ExceptionTester<UnknownOptionException>.TestSerializationConstructor((info, context) => new UnknownOptionException(info, context));
        }
    }
}
