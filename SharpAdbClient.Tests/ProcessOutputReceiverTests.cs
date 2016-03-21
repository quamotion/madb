//-----------------------------------------------------------------------
// <copyright file="ProcessOutputReceiverTests.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System.IO;
using System.Linq;

namespace SharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="ProcessOutputReceiver"/> class.
    /// </summary>
    [TestClass]
    public class ProcessOutputReceiverTests
    {
        /// <summary>
        /// Tests the <see cref="ProcessOutputReceiver"/> class in a scenario where all output
        /// is sent on a line by line basis.
        /// </summary>
        [TestMethod]
        public void ProcessOutputTest()
        {
            string output = @"USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME
root      1     0     852    628   c1157816 0805d0d6 S /init
root      2     0     0      0     c10589a4 00000000 S kthreadd
root      127   2     0      0     c1052a3b 00000000 S ext4-dio-unwrit
root      129   1     1572   4     c11850d6 0805f0bb S /sbin/healthd
shell     145   1     1552   584   c135a860 b7649fe6 S /system/bin/sh
root      146   1     4764   260   ffffffff 0806ba20 S /sbin/adbd
system    452   138   511452 44104 ffffffff b765ff1b S system_server
root      470   2     0      0     c1053187 00000000 S kworker/1:1H
root      476   1     1544   616   c104b8ee b76b75b6 S /system/bin/sh
root      478   476   1412   204   c105c979 b771a1e1 S zygotelaunch
u0_a33    664   138   449648 22100 ffffffff b765ff1b S com.android.music
u0_a5     698   138   451988 32252 ffffffff b765ff1b S android.process.media
root      710   1     1312   416   c1157816 b7697fe6 S /system/bin/logwrapper
root      715   710   1544   612   c104b8ee b76405b6 S /system/bin/sh
wifi      722   715   5812   2152  c1157816 b74c2610 S /system/bin/wpa_supplicant
root      731   2     0      0     c1053187 00000000 S kworker/0:1H
root      754   1     1312   416   c1157816 b7719fe6 S /system/bin/logwrapper
dhcp      755   754   1624   740   c1157816 b770dfe6 S /system/bin/dhcpcd
u0_a4     762   138   451852 21908 ffffffff b765ff1b S com.android.dialer
root      991   146   1556   696   c104b8ee b76345b6 S /system/bin/sh
root      1044  991   1868   504   00000000 b76c0fe6 R ps";

            ProcessOutputReceiver receiver = new ProcessOutputReceiver();

            using (StringReader reader = new StringReader(output))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    receiver.AddOutput(line);
                }
            }

            receiver.Flush();
            Assert.AreEqual(21, receiver.Processes.Count);
            Assert.AreEqual("/init", receiver.Processes.First().Name);
            Assert.AreEqual("ps", receiver.Processes.Last().Name);
        }

        /// <summary>
        /// Tests the <see cref="ProcessOutputReceiver"/> class in a scenario where all output
        /// is sent in a single packet.
        /// </summary>
        [TestMethod]
        public void ProcessMultiLineOutputTest()
        {
            string content = @"root      1     0     852    628   c1157816 0805d0d6 S /init
root      2     0     0      0     c10589a4 00000000 S kthreadd
root      3     2     0      0     c105fd59 00000000 S ksoftirqd/0";

            ProcessOutputReceiver receiver = new ProcessOutputReceiver();

            StringReader reader = new StringReader(content);

            while (reader.Peek() >= 0)
            {
                receiver.AddOutput(reader.ReadLine());
            }

            receiver.Flush();
            Assert.AreEqual(2, receiver.Processes.Count);
        }

        /// <summary>
        /// Tests the <see cref="ProcessOutputReceiver"/> class in a scenario where the output is in 
        /// a limited format (not all fields are present)
        /// </summary>
        [TestMethod]
        public void ProcessLimitedOutputTest()
        {
            string content = @"  PID USER       VSZ STAT COMMAND
    1 root       340 S    /init
    2 root         0 SW<  [kthreadd]";

            ProcessOutputReceiver receiver = new ProcessOutputReceiver();

            StringReader reader = new StringReader(content);

            while (reader.Peek() >= 0)
            {
                receiver.AddOutput(reader.ReadLine());
            }

            receiver.Flush();
            Assert.AreEqual(2, receiver.Processes.Count);
            Assert.AreEqual("/init", receiver.Processes.First().Name);
            Assert.AreEqual("kthreadd", receiver.Processes.Last().Name);
        }

        /// <summary>
        /// Tests the parsing of the process output in a context where the <see cref="AndroidProcess.WChan"/>
        /// field can contain string data (instead of hexadecimal numbers).
        /// </summary>
        [TestMethod]
        public void ProcessStringWchanOutputTest()
        {
            string content = @"USER      PID   PPID  VSIZE  RSS   WCHAN            PC  NAMEroot      1     0     2476   980   SyS_epoll_ 00000000 S /initroot      2     0     0      0       kthreadd 00000000 S kthreadd";

            ProcessOutputReceiver receiver = new ProcessOutputReceiver();

            StringReader reader = new StringReader(content);

            while (reader.Peek() >= 0)
            {
                receiver.AddOutput(reader.ReadLine());
            }

            receiver.Flush();
            Assert.AreEqual(2, receiver.Processes.Count);
            Assert.AreEqual("SyS_epoll_", receiver.Processes.First().WChan);
            Assert.AreEqual("kthreadd", receiver.Processes.Last().WChan);
        }
    }
}
