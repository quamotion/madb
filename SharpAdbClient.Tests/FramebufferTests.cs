using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class FramebufferTests
    {
        [Fact(Skip = "PerformanceTest")]
        public void GetFramebufferAsyncPerformanceTest()
        {
            var device = AdbClient.Instance.GetDevices().First();

            while (true)
            {
                var img = AdbClient.Instance.GetFrameBufferAsync(device, CancellationToken.None).Result;
            }
        }

        [Fact(Skip = "PerformanceTest")]
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

