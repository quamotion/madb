using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbResponseTests
    {
        [TestMethod]
        public void EqualsTest()
        {
            AdbResponse first = new AdbResponse()
            {
                IOSuccess = false,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            AdbResponse second = new AdbResponse()
            {
                IOSuccess = true,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            Assert.IsFalse(first.Equals("some string"));
            Assert.IsFalse(first.Equals(second));
            Assert.IsTrue(first.Equals(first));
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            AdbResponse first = new AdbResponse()
            {
                IOSuccess = false,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            AdbResponse second = new AdbResponse()
            {
                IOSuccess = false,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }

        [TestMethod]
        public void ToStringTest()
        {
            Assert.AreEqual("OK", AdbResponse.OK.ToString());
            Assert.AreEqual("Error: Huh?", AdbResponse.FromError("Huh?").ToString());
        }
    }
}
