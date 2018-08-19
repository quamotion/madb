using System;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class AdbServerStatusTests
    {
        [Fact]
        public void ToStringTest()
        {
            AdbServerStatus s = new AdbServerStatus()
            {
                IsRunning = true,
                Version = new Version(1, 0, 32)
            };

            Assert.Equal("Version 1.0.32 of the adb daemon is running.", s.ToString());

            s.IsRunning = false;

            Assert.Equal("The adb daemon is not running.", s.ToString());
        }
    }
}
