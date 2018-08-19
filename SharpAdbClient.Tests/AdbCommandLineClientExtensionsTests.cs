using Xunit;
using Moq;
using System;
using System.IO;

namespace SharpAdbClient.Tests
{
    public class AdbCommandLineClientExtensionsTests
    {
        [Fact]
        public void EnsureIsValidAdbFileNullValueTest()
        {
            Assert.Throws< ArgumentNullException>(() => AdbCommandLineClientExtensions.EnsureIsValidAdbFile(null, "adb.exe"));
        }

        [Fact]
        public void EnsureIsValidAdbFileInvalidFileTest()
        {
            var clientMock = new Mock<IAdbCommandLineClient>();
            clientMock.Setup(c => c.IsValidAdbFile(It.IsAny<string>())).Returns(false);

            var client = clientMock.Object;

            Assert.Throws<FileNotFoundException>(() => client.EnsureIsValidAdbFile("xyz.exe"));
        }
    }
}
