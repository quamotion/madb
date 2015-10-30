using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class AndroidDebugBridgeTests
    {
        [TestInitialize]
        public void Initialize()
        {
            AndroidDebugBridge.AdbOsLocation = @"c:\program files (x86)\quamotion\ext\adk\platform-tools\adb.exe";
        }

        [TestMethod]
        public void StartAdbTest()
        {
            List<string> standardOutput = new List<string>();
            List<string> standardError = new List<string>();

            standardOutput.Add("Android Debug Bridge version 1.0.32");
            standardOutput.Add("");

            var version = AndroidDebugBridge.GetAdbVersion(standardOutput, standardError);
            Assert.AreEqual(new Version("1.0.32"), version);
        }
    }
}
