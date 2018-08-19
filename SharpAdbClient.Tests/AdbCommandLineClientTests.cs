using SharpAdbClient.Exceptions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbCommandLineClient"/> class.
    /// </summary>
    public class AdbCommandLineClientTests
    {
        [Fact]
        public void GetVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            commandLine.Version = new Version(1, 0, 32);

            Assert.Equal(new Version(1, 0, 32), commandLine.GetVersion());
        }

        [Fact]
        public void GetVersionNullTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            commandLine.Version = null;
            Assert.Throws<AdbException>(() => commandLine.GetVersion());
        }

        [Fact]
        public void GetOutdatedVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            commandLine.Version = new Version(1, 0, 1);

            Assert.Throws<AdbException>(() => commandLine.GetVersion());
        }

        [Fact]
        public void StartServerTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            Assert.False(commandLine.ServerStarted);
            commandLine.StartServer();
            Assert.True(commandLine.ServerStarted);
        }
    }
}
