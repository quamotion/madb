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
        [TestMethod]
        public void ParseHeaderTest()
        {
            var header = AndroidProcess.ParseHeader("USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME");
            Assert.AreEqual(0, header.UserIndex);
            Assert.AreEqual(1, header.ProcessIdIndex);
            Assert.AreEqual(2, header.ParentProcessIdIndex);
            Assert.AreEqual(3, header.VirtualSizeIndex);
            Assert.AreEqual(4, header.ResidentSetSizeIndex);
            Assert.AreEqual(5, header.WChanIndex);
            Assert.AreEqual(6, header.PcIndex);
            Assert.AreEqual(7, header.StateIndex);
            Assert.AreEqual(8, header.NameIndex);
        }

        [TestMethod]
        public void ParseHeaderTest2()
        {
            var header = AndroidProcess.ParseHeader("  PID USER       VSZ STAT COMMAND");
            Assert.AreEqual(1, header.UserIndex);
            Assert.AreEqual(0, header.ProcessIdIndex);
            Assert.AreEqual(-1, header.ParentProcessIdIndex);
            Assert.AreEqual(2, header.VirtualSizeIndex);
            Assert.AreEqual(-1, header.ResidentSetSizeIndex);
            Assert.AreEqual(-1, header.WChanIndex);
            Assert.AreEqual(-1, header.PcIndex);
            Assert.AreEqual(3, header.StateIndex);
            Assert.AreEqual(4, header.NameIndex);
        }

        /// <summary>
        /// Tests the <see cref="AndroidProcess.Parse(string)"/> method.
        /// </summary>
        [TestMethod]
        public void ParseTest()
        {
            // USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME
            // system    479   138   446284 21100 ffffffff b765ffe6 S com.microsoft.xde.donatelloservice
            string headerLine = "USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME";
            string line = @"system    479   138   446284 21100 ffffffff b765ffe6 S com.microsoft.xde.donatelloservice";

            var header = AndroidProcess.ParseHeader(headerLine);
            var process = AndroidProcess.Parse(line, header);

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

        [TestMethod]
        public void ParseTest2()
        {
            //   PID USER       VSZ STAT COMMAND
            //    1 root       340 S    /init
            string headerLine = "  PID USER       VSZ STAT COMMAND";
            string line = @"    2 root       340 S    [kthreadd]";

            var header = AndroidProcess.ParseHeader(headerLine);
            var process = AndroidProcess.Parse(line, header);

            Assert.AreEqual("root", process.User);
            Assert.AreEqual(2, process.ProcessId);
            Assert.AreEqual(-1, process.ParentProcessId);
            Assert.AreEqual(340, process.VirtualSize);
            Assert.AreEqual(-1, process.ResidentSetSize);
            Assert.AreEqual(uint.MaxValue, process.WChan);
            Assert.AreEqual(uint.MaxValue, process.Pc);
            Assert.AreEqual(AndroidProcessState.S, process.State);
            Assert.AreEqual("kthreadd", process.Name);
        }
    }
}
