using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests.Extensions
{
    [TestClass]
    public class ArrayHelperTests
    {
        [TestMethod]
        public void Swap32bitFromArrayTest()
        {
            byte[] value = new byte[] { 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };
            var number = value.Swap32BitFromArray(1);
            Assert.AreEqual(0x32547698, number);
        }

        [TestMethod]
        public void SwapU16bitFromArrayTest()
        {
            byte[] value = new byte[] { 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };
            var number = value.SwapU16BitFromArray(1);
            Assert.AreEqual(0x7698, number);
        }
    }
}
