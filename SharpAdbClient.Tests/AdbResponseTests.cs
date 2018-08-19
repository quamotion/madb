using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    public class AdbResponseTests
    {
        [Fact]
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

            Assert.False(first.Equals("some string"));
            Assert.False(first.Equals(second));
            Assert.True(first.Equals(first));
        }

        [Fact]
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

            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void ToStringTest()
        {
            Assert.Equal("OK", AdbResponse.OK.ToString());
            Assert.Equal("Error: Huh?", AdbResponse.FromError("Huh?").ToString());
        }
    }
}
