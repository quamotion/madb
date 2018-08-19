//-----------------------------------------------------------------------
// <copyright file="AndroidProcessTests.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Xunit;
using SharpAdbClient.DeviceCommands;

namespace SharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AndroidProcess"/> class.
    /// </summary>
    public class AndroidProcessTests
    {
        /// <summary>
        /// Tests the <see cref="AndroidProcess.Parse(string)"/> method.
        /// </summary>
        [Fact]
        public void ParseTest()
        {
            string line = @"1 (init) S 0 0 0 0 -1 1077936384 1467 168323 0 38 12 141 863 249 20 0 1 0 4 2535424 245 4294967295 1 1 0 0 0 0 0 0 65536 4294967295 0 0 17 3 0 0 0 0 0 0 0 0 0 0 0 0 0";

            var process = AndroidProcess.Parse(line);

            Assert.Equal(1, process.ProcessId);
            Assert.Equal(0, process.ParentProcessId);
            Assert.Equal(2535424ul, process.VirtualSize);
            Assert.Equal(245, process.ResidentSetSize);
            Assert.Equal(4294967295ul, process.WChan);
            Assert.Equal(AndroidProcessState.S, process.State);
            Assert.Equal("init", process.Name);
        }

        [Fact]
        public void ParseWithSpaceTest()
        {
            string line = @"194(irq/432-mdm sta) S 2 0 0 0 - 1 2130240 0 0 0 0 0 1 0 0 - 51 0 1 0 172 0 0 4294967295 0 0 0 0 0 0 0 2147483647 0 4294967295 0 0 17 1 50 1 0 0 0 0 0 0 0 0 0 0 0";

            var process = AndroidProcess.Parse(line);

            Assert.Equal(194, process.ProcessId);
            Assert.Equal(2, process.ParentProcessId);
            Assert.Equal(0ul, process.VirtualSize);
            Assert.Equal(172, process.ResidentSetSize);
            Assert.Equal(2147483647ul, process.WChan);
            Assert.Equal(AndroidProcessState.S, process.State);
            Assert.Equal("irq/432-mdm sta", process.Name);
        }
    }
}
