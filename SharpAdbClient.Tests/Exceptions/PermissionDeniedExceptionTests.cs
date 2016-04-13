using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    [TestClass]
    public class PermissionDeniedExceptionTests
    {
        [TestMethod]
        public void TestEmptyConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestEmptyConstructor(() => new PermissionDeniedException());
        }

        [TestMethod]
        public void TestMessageConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestMessageConstructor((message) => new PermissionDeniedException(message));
        }

        [TestMethod]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestMessageAndInnerConstructor((message, inner) => new PermissionDeniedException(message, inner));
        }

        [TestMethod]
        public void TestSerializationConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestSerializationConstructor((info, context) => new PermissionDeniedException(info, context));
        }
    }
}
