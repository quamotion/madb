using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void GetFileNameExtensionTest()
        {
            String result = LinuxPath.GetExtension("/system/busybox");
            Assert.AreEqual<string>("", result);

            result = LinuxPath.GetExtension("/");
            Assert.AreEqual<string>("", result);

            result = LinuxPath.GetExtension("/system/xbin/");
            Assert.AreEqual<string>("", result);

            result = LinuxPath.GetExtension("/system/xbin/file.ext");
            Assert.AreEqual<string>(".ext", result);
        }

        [TestMethod]
        public void GetFileNameWithoutExtensionTest()
        {
            String result = LinuxPath.GetFileNameWithoutExtension("/system/busybox");
            Assert.AreEqual<string>("busybox", result);

            result = LinuxPath.GetFileNameWithoutExtension("/");
            Assert.AreEqual<string>("", result);

            result = LinuxPath.GetFileNameWithoutExtension("/system/xbin/");
            Assert.AreEqual<string>("", result);

            result = LinuxPath.GetFileNameWithoutExtension("/system/xbin/file.ext");
            Assert.AreEqual<string>("file", result);
        }

        [TestMethod]
        public void ChangeExtensionTest()
        {
            String result = LinuxPath.ChangeExtension("/system/busybox", "foo");
            Assert.AreEqual<string>("/system/busybox.foo", result);

            result = LinuxPath.ChangeExtension("/system/xbin/file.ext", "myext");
            Assert.AreEqual<string>("/system/xbin/file.myext", result);

            result = LinuxPath.ChangeExtension("/system/xbin/file.ext", "");
            Assert.AreEqual<string>("/system/xbin/file", result);

            result = LinuxPath.ChangeExtension("/system/busybox.foo", "");
            Assert.AreEqual<string>("/system/busybox", result);
        }

        [TestMethod]
        public void GetPathWithoutFileTest()
        {
            String result = LinuxPath.GetPathWithoutFile("/system/busybox");
            Assert.AreEqual<string>("/system/", result);

            result = LinuxPath.GetPathWithoutFile("/system/xbin/");
            Assert.AreEqual<string>("/system/xbin/", result);

            result = LinuxPath.GetPathWithoutFile("/system/xbin/file.ext");
            Assert.AreEqual<string>("/system/xbin/", result);
        }

        [TestMethod]
        public void GetPathRootTest()
        {
            String result = LinuxPath.GetPathRoot("/system/busybox");
            Assert.AreEqual<string>("/", result);

            result = LinuxPath.GetPathRoot("/system/xbin/");
            Assert.AreEqual<string>("/", result);

            result = LinuxPath.GetPathRoot("/system/xbin/file.ext");
            Assert.AreEqual<string>("/", result);
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

        [TestMethod]
        public void HasExtensionTest()
        {
            bool result = LinuxPath.HasExtension("/system/busybox");
            Assert.AreEqual<bool>(false, result);

            result = LinuxPath.HasExtension("/system/xbin.foo/");
            Assert.AreEqual<bool>(false, result);

            result = LinuxPath.HasExtension("system/xbin/file.ext");
            Assert.AreEqual<bool>(true, result);
        }
    }
}
