using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class FramebufferTests
    {
        [TestCategory("PerformanceTest")]
        [TestMethod]
        public void GetFramebufferAsyncPerformanceTest()
        {
            var device = AdbClient.Instance.GetDevices().First();

            while (true)
            {
                var img = AdbClient.Instance.GetFrameBufferAsync(device, CancellationToken.None).Result;
            }
        }

        [TestCategory("PerformanceTest")]
        [TestMethod]
        public async Task RefreshFramebufferAsyncPerformanceTest()
        {
            var device = AdbClient.Instance.GetDevices().First();

            Framebuffer framebuffer = AdbClient.Instance.CreateRefreshableFramebuffer(device);
            while (true)
            {
                await framebuffer.RefreshAsync(CancellationToken.None).ConfigureAwait(false);
                // var img = framebuffer.ToImage();
            }
        }
    }
}

