using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbCommandLineClientExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnsureIsValidAdbFileNullValueTest()
        {
            AdbCommandLineClientExtensions.EnsureIsValidAdbFile(null, "adb.exe");
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void EnsureIsValidAdbFileInvalidFileTest()
        {
            var clientMock = new Mock<IAdbCommandLineClient>();
            clientMock.Setup(c => c.IsValidAdbFile(It.IsAny<string>())).Returns(false);

            var client = clientMock.Object;

            client.EnsureIsValidAdbFile("xyz.exe");
        }
    }
}
