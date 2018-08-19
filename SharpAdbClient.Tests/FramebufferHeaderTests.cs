using Xunit;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SharpAdbClient.Tests
{
    public class FramebufferHeaderTests
    {
        [Fact]
        public void ReadFramebufferTest()
        {
            var data = File.ReadAllBytes("framebufferheader.bin");

            var header = FramebufferHeader.Read(data);

            Assert.Equal(8u, header.Alpha.Length);
            Assert.Equal(24u, header.Alpha.Offset);
            Assert.Equal(8u, header.Green.Length);
            Assert.Equal(8u, header.Green.Offset);
            Assert.Equal(8u, header.Red.Length);
            Assert.Equal(0u, header.Red.Offset);
            Assert.Equal(8u, header.Blue.Length);
            Assert.Equal(16u, header.Blue.Offset);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(2560u, header.Height);
            Assert.Equal(1440u, header.Width);
            Assert.Equal(1u, header.Version);
            Assert.Equal(0u, header.ColorSpace);
        }

        [Fact]
        public void ReadFramebufferv2Test()
        {
            var data = File.ReadAllBytes("framebufferheader-v2.bin");

            var header = FramebufferHeader.Read(data);

            Assert.Equal(8u, header.Alpha.Length);
            Assert.Equal(24u, header.Alpha.Offset);
            Assert.Equal(8u, header.Green.Length);
            Assert.Equal(8u, header.Green.Offset);
            Assert.Equal(8u, header.Red.Length);
            Assert.Equal(0u, header.Red.Offset);
            Assert.Equal(8u, header.Blue.Length);
            Assert.Equal(16u, header.Blue.Offset);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(1920u, header.Height);
            Assert.Equal(1080u, header.Width);
            Assert.Equal(2u, header.Version);
            Assert.Equal(0u, header.ColorSpace);
        }

        [Fact]
        public void ToImageTest()
        {
            var data = File.ReadAllBytes("framebufferheader.bin");
            var header = FramebufferHeader.Read(data);
            header.Width = 1;
            header.Height = 1;

            var framebuffer = File.ReadAllBytes("framebuffer.bin");
            using (var image = (Bitmap)header.ToImage(framebuffer))
            {
                Assert.NotNull(image);
                Assert.Equal(PixelFormat.Format32bppArgb, image.PixelFormat);

                Assert.Equal(1, image.Width);
                Assert.Equal(1, image.Height);

                var pixel = image.GetPixel(0, 0);
                Assert.Equal(0x35, pixel.R);
                Assert.Equal(0x4a, pixel.G);
                Assert.Equal(0x4c, pixel.B);
                Assert.Equal(0xff, pixel.A);
            }
        }

        [Fact]
        public void ToImageEmptyTest()
        {
            var data = File.ReadAllBytes("framebufferheader-empty.bin");
            var header = FramebufferHeader.Read(data);

            var framebuffer = new byte[] { };

            var image = header.ToImage(framebuffer);
            Assert.Null(image);
        }
    }
}
