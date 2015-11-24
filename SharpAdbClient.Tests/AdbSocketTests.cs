using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbSocketTests
    {
        [TestMethod]
        public void IsOkayTest()
        {
            var okay = Encoding.ASCII.GetBytes("OKAY");
            var fail = Encoding.ASCII.GetBytes("FAIL");

            Assert.IsTrue(AdbSocket.IsOkay(okay));
            Assert.IsFalse(AdbSocket.IsOkay(fail));
        }

    }
}
