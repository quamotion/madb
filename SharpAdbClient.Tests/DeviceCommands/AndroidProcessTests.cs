using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests.DeviceCommands
{
    [TestClass]
    public class AndroidProcessTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParseNullTest()
        {
            AndroidProcess.Parse(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ParseTooFewPartsTest()
        {
            AndroidProcess.Parse("1 (init) S 0 0 0 0 -1 1077944576 2680 83280 0 179 0 67 16 39 20 0 1 0 2 17735680 143 18446744073709551615 134512640 135145076 ");
        }

        [TestMethod]
        public void ToStringTest()
        {
            AndroidProcess p = new AndroidProcess();
            p.ProcessId = 1;
            p.Name = "init";

            Assert.AreEqual("init (1)", p.ToString());
        }
    }
}
