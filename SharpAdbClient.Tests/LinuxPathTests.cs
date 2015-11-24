using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class LinuxPathTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckInvalidPathCharsNullTest()
        {
            LinuxPath.CheckInvalidPathChars(null);
        }

        [TestMethod]
        public void CheckInvalidPathCharsTest()
        {
            // Should not throw an exception.
            LinuxPath.CheckInvalidPathChars("/var/test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CheckInvalidPathCharsTest2()
        {
            // Should throw an exception.
            LinuxPath.CheckInvalidPathChars("/var/test > out");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CheckInvalidPathCharsTest3()
        {
            // Should throw an exception.
            LinuxPath.CheckInvalidPathChars("\t/var/test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CombineNullTest()
        {
            LinuxPath.Combine(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CombineNullTest2()
        {
            LinuxPath.Combine(new string[] { "/test", "hi", null });
        }

        [TestMethod]
        public void CombineTest()
        {
            String result = LinuxPath.Combine("/system", "busybox");
            Assert.AreEqual<string>("/system/busybox", result);

            result = LinuxPath.Combine("/system/", "busybox");
            Assert.AreEqual<string>("/system/busybox", result);

            result = LinuxPath.Combine("/system/xbin", "busybox");
            Assert.AreEqual<string>("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system/xbin/", "busybox");
            Assert.AreEqual<string>("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system//xbin", "busybox");
            Assert.AreEqual<string>("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system", "xbin", "busybox");
            Assert.AreEqual<string>("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system", "xbin", "really", "long", "path", "to", "nothing");
            Assert.AreEqual<string>("/system/xbin/really/long/path/to/nothing", result);
        }

        [TestMethod]
        public void CombineCurrentDirTest()
        {
            var result = LinuxPath.Combine(".", "test.txt");
            Assert.AreEqual("./test.txt", result);
        }

        [TestMethod]
        public void GetDirectoryNameTest()
        {
            String result = LinuxPath.GetDirectoryName("/system/busybox");
            Assert.AreEqual<string>("/system/", result);

            result = LinuxPath.GetDirectoryName("/");
            Assert.AreEqual<string>("/", result);

            result = LinuxPath.GetDirectoryName("/system/xbin/");
            Assert.AreEqual<string>("/system/xbin/", result);

            result = LinuxPath.GetDirectoryName("echo");
            Assert.AreEqual<string>("./", result);

            result = LinuxPath.GetDirectoryName(null);
            Assert.AreEqual<string>(null, result);
        }

        [TestMethod]
        public void GetFileNameTest()
        {
            String result = LinuxPath.GetFileName("/system/busybox");
            Assert.AreEqual<string>("busybox", result);

            result = LinuxPath.GetFileName("/");
            Assert.AreEqual<string>("", result);

            result = LinuxPath.GetFileName("/system/xbin/");
            Assert.AreEqual<string>("", result);

            result = LinuxPath.GetFileName("/system/xbin/file.ext");
            Assert.AreEqual<string>("file.ext", result);

            result = LinuxPath.GetFileName(null);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void IsPathRootedTest()
        {
            bool result = LinuxPath.IsPathRooted("/system/busybox");
            Assert.AreEqual<bool>(true, result);

            result = LinuxPath.IsPathRooted("/system/xbin/");
            Assert.AreEqual<bool>(true, result);

            result = LinuxPath.IsPathRooted("system/xbin/");
            Assert.AreEqual<bool>(false, result);
        }
    }
}
