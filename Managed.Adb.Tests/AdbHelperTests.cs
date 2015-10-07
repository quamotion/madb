using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class AdbHelperTests
    {
        [TestMethod]
        public void FormAdbRequestTest()
        {
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0009host:kill\n"), AdbHelper.FormAdbRequest("host:kill"));
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("000Chost:version\n"), AdbHelper.FormAdbRequest("host:version"));
        }

        [TestMethod]
        public void CreateAdbForwardRequestTest()
        {
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0008tcp:1984\n"), AdbHelper.CreateAdbForwardRequest(null, 1984));
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0012tcp:1981:127.0.0.1\n"), AdbHelper.CreateAdbForwardRequest("127.0.0.1", 1981));
        }
    }
}
