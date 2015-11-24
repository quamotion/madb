//-----------------------------------------------------------------------
// <copyright file="AndroidProcessTests.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;

namespace SharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AndroidProcess"/> class.
    /// </summary>
    [TestClass]
    public class AndroidProcessTests
    {
        /// <summary>
        /// Tests the <see cref="AndroidProcess.Parse(string)"/> method.
        /// </summary>
        [TestMethod]
        public void ParseTest()
        {
            // USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME
            // system    479   138   446284 21100 ffffffff b765ffe6 S com.microsoft.xde.donatelloservice
            string line = @"system    479   138   446284 21100 ffffffff b765ffe6 S com.microsoft.xde.donatelloservice";

            var process = AndroidProcess.Parse(line);

            Assert.AreEqual("system", process.User);
            Assert.AreEqual(479, process.ProcessId);
            Assert.AreEqual(138, process.ParentProcessId);
            Assert.AreEqual(446284, process.VirtualSize);
            Assert.AreEqual(21100, process.ResidentSetSize);
            Assert.AreEqual(0xffffffff, process.WChan);
            Assert.AreEqual(0xb765ffe6, process.Pc);
            Assert.AreEqual(AndroidProcessState.S, process.State);
            Assert.AreEqual("com.microsoft.xde.donatelloservice", process.Name);
        }
    }
}
