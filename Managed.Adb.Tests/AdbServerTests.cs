using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class AdbServerTests
    {
        [TestMethod]
        public void GetStatusTest()
        {
            var status = AdbServer.GetStatus();
            Assert.IsTrue(status.IsRunning);
            Assert.AreEqual(new Version(1, 0, 32), status.Version);
        }
    }
}
