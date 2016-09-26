using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class FramebufferHeaderTests
    {
        [TestMethod]
        [DeploymentItem(@"framebufferheader.bin")]
        public void ReadFramebufferTest()
        {
            var data = File.ReadAllBytes("framebufferheader.bin");

            var header = FramebufferHeader.Read(data);

            Assert.AreEqual(8u, header.Alpha.Length);
            Assert.AreEqual(24u, header.Alpha.Offset);
            Assert.AreEqual(8u, header.Green.Length);
            Assert.AreEqual(8u, header.Green.Offset);
            Assert.AreEqual(8u, header.Red.Length);
            Assert.AreEqual(0u, header.Red.Offset);
            Assert.AreEqual(8u, header.Blue.Length);
            Assert.AreEqual(16u, header.Blue.Offset);
            Assert.AreEqual(32u, header.Bpp);
            Assert.AreEqual(2560u, header.Height);
            Assert.AreEqual(1440u, header.Width);
            Assert.AreEqual(1u, header.Version);
        }

        [TestMethod]
        [DeploymentItem("framebuffer.bin")]
        [DeploymentItem(@"framebufferheader.bin")]
        public void ToImageTest()
        {
            var data = File.ReadAllBytes("framebufferheader.bin");
            var header = FramebufferHeader.Read(data);
            header.Width = 1;
            header.Height = 1;

            var framebuffer = File.ReadAllBytes("framebuffer.bin");
            using (var image = (Bitmap)header.ToImage(framebuffer))
            {
                Assert.IsNotNull(image);
                Assert.AreEqual(PixelFormat.Format32bppArgb, image.PixelFormat);

                Assert.AreEqual(1, image.Width);
                Assert.AreEqual(1, image.Height);

                var pixel = image.GetPixel(0, 0);
                Assert.AreEqual(0x35, pixel.R);
                Assert.AreEqual(0x4a, pixel.G);
                Assert.AreEqual(0x4c, pixel.B);
                Assert.AreEqual(0xff, pixel.A);
            }
        }

        [TestMethod]
        [DeploymentItem(@"framebufferheader-empty.bin")]
        public void ToImageEmptyTest()
        {
            var data = File.ReadAllBytes("framebufferheader-empty.bin");
            var header = FramebufferHeader.Read(data);

            var framebuffer = new byte[] { };

            var image = header.ToImage(framebuffer);
            Assert.IsNull(image);
        }
    }
}
