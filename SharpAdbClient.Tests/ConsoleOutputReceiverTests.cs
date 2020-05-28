using Xunit;
using SharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    public class ConsoleOutputReceiverTests
    {
        [Fact]
        public void ToStringTest()
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            receiver.AddOutput("Hello, World!");
            receiver.AddOutput("See you!");

            receiver.Flush();

            Assert.Equal("Hello, World!\r\nSee you!\r\n",
                receiver.ToString(),
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToStringIgnoredLineTest()
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            receiver.AddOutput("#Hello, World!");
            receiver.AddOutput("See you!");

            receiver.Flush();

            Assert.Equal("See you!\r\n",
                receiver.ToString(),
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToStringIgnoredLineTest2()
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            receiver.AddOutput("Hello, World!");
            receiver.AddOutput("$See you!");

            receiver.Flush();

            Assert.Equal("Hello, World!\r\n",
                receiver.ToString(),
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void TrowOnErrorTest()
        {
            AssertTrowsException<FileNotFoundException>("/dev/test: not found");
            AssertTrowsException<FileNotFoundException>("No such file or directory");
            AssertTrowsException<UnknownOptionException>("Unknown option -h");
            AssertTrowsException<CommandAbortingException>("/dev/test: Aborting.");
            AssertTrowsException<FileNotFoundException>("/dev/test: applet not found");
            AssertTrowsException<PermissionDeniedException>("/dev/test: permission denied");
            AssertTrowsException<PermissionDeniedException>("/dev/test: access denied");

            // Should not thrown an exception
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            receiver.ThrowOnError("Stay calm and watch cat movies.");
        }

        private void AssertTrowsException<T>(string line)
            where T : Exception
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            Assert.Throws<T>(() => receiver.ThrowOnError(line));
        }
    }
}
