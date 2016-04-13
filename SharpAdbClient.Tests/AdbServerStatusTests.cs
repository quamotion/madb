using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbServerStatusTests
    {
        [TestMethod]
        public void ToStringTest()
        {
            AdbServerStatus s = new AdbServerStatus()
            {
                IsRunning = true,
                Version = new Version(1, 0, 32)
            };

            Assert.AreEqual("Version 1.0.32 of the adb daemon is running.", s.ToString());

            s.IsRunning = false;

            Assert.AreEqual("The adb daemon is not running.", s.ToString());
        }
    }
}
