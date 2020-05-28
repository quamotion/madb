using SharpAdbClient.DeviceCommands;
using System;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class LinuxPathTests
    {
        [Fact]
        public void CheckInvalidPathCharsNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => LinuxPath.CheckInvalidPathChars(null));
        }

        [Fact]
        public void CheckInvalidPathCharsTest()
        {
            // Should not throw an exception.
            LinuxPath.CheckInvalidPathChars("/var/test");
        }

        [Fact]
        public void CheckInvalidPathCharsTest2()
        {
            // Should throw an exception.
            Assert.Throws<ArgumentException>(() => LinuxPath.CheckInvalidPathChars("/var/test > out"));
        }

        [Fact]
        public void CheckInvalidPathCharsTest3()
        {
            // Should throw an exception.
            Assert.Throws<ArgumentException>(() => LinuxPath.CheckInvalidPathChars("\t/var/test"));
        }

        [Fact]
        public void CombineNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => LinuxPath.Combine(null));
        }

        [Fact]
        public void CombineNullTest2()
        {
            Assert.Throws<ArgumentNullException>(() => LinuxPath.Combine(new string[] { "/test", "hi", null }));
        }

        [Fact]
        public void CombineTest()
        {
            String result = LinuxPath.Combine("/system", "busybox");
            Assert.Equal("/system/busybox", result);

            result = LinuxPath.Combine("/system/", "busybox");
            Assert.Equal("/system/busybox", result);

            result = LinuxPath.Combine("/system/xbin", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system/xbin/", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system//xbin", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system", "xbin", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system", "xbin", "really", "long", "path", "to", "nothing");
            Assert.Equal("/system/xbin/really/long/path/to/nothing", result);
        }

        [Fact]
        public void CombineCurrentDirTest()
        {
            var result = LinuxPath.Combine(".", "test.txt");
            Assert.Equal("./test.txt", result);
        }

        [Fact]
        public void GetDirectoryNameTest()
        {
            String result = LinuxPath.GetDirectoryName("/system/busybox");
            Assert.Equal("/system/", result);

            result = LinuxPath.GetDirectoryName("/");
            Assert.Equal("/", result);

            result = LinuxPath.GetDirectoryName("/system/xbin/");
            Assert.Equal("/system/xbin/", result);

            result = LinuxPath.GetDirectoryName("echo");
            Assert.Equal("./", result);

            result = LinuxPath.GetDirectoryName(null);
            Assert.Null(result);
        }

        [Fact]
        public void GetFileNameTest()
        {
            String result = LinuxPath.GetFileName("/system/busybox");
            Assert.Equal("busybox", result);

            result = LinuxPath.GetFileName("/");
            Assert.Equal("", result);

            result = LinuxPath.GetFileName("/system/xbin/");
            Assert.Equal("", result);

            result = LinuxPath.GetFileName("/system/xbin/file.ext");
            Assert.Equal("file.ext", result);

            result = LinuxPath.GetFileName(null);
            Assert.Null(result);
        }

        [Fact]
        public void IsPathRootedTest()
        {
            Assert.True(LinuxPath.IsPathRooted("/system/busybox"));
            Assert.True(LinuxPath.IsPathRooted("/system/xbin/"));
            Assert.False(LinuxPath.IsPathRooted("system/xbin/"));
        }
    }
}
