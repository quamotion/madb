using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;

namespace SharpAdbClient.Tests.Exceptions
{
    [TestClass]
    public class DeviceNotFoundExceptionTests
    {
        [TestMethod]
        public void TestEmptyConstructor()
        {
            ExceptionTester<DeviceNotFoundException>.TestEmptyConstructor(() => new DeviceNotFoundException());
        }

        [TestMethod]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<DeviceNotFoundException>.TestMessageAndInnerConstructor((message, inner) => new DeviceNotFoundException(message, inner));
        }

        [TestMethod]
        public void TestSerializationConstructor()
        {
            ExceptionTester<DeviceNotFoundException>.TestSerializationConstructor((info, context) => new DeviceNotFoundException(info, context));
        }
    }
}
