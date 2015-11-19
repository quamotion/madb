using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class LinuxPathTests
    {

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
