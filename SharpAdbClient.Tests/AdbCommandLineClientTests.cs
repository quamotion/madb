using SharpAdbClient.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbCommandLineClient"/> class.
    /// </summary>
    [TestClass]
    public class AdbCommandLineClientTests
    {
        [TestMethod]
        public void GetVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            commandLine.Version = new Version(1, 0, 32);

            Assert.AreEqual(new Version(1, 0, 32), commandLine.GetVersion());
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
        public void GetVersionNullTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            commandLine.Version = null;
            commandLine.GetVersion();
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
        public void GetOutdatedVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            commandLine.Version = new Version(1, 0, 1);

            commandLine.GetVersion();
        }

        [TestMethod]
        public void StartServerTest()
        {
            DummyAdbCommandLineClient commandLine = new DummyAdbCommandLineClient();
            Assert.IsFalse(commandLine.ServerStarted);
            commandLine.StartServer();
            Assert.IsTrue(commandLine.ServerStarted);
        }
    }
}
