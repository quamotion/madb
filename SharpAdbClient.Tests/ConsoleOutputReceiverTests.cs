using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class ConsoleOutputReceiverTests
    {
        [TestMethod]
        public void ToStringTest()
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            receiver.AddOutput("Hello, World!");
            receiver.AddOutput("See you!");

            receiver.Flush();

            Assert.AreEqual("Hello, World!\r\nSee you!\r\n",
                receiver.ToString());
        }

        [TestMethod]
        public void ToStringIgnoredLineTest()
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            receiver.AddOutput("#Hello, World!");
            receiver.AddOutput("See you!");

            receiver.Flush();

            Assert.AreEqual("See you!\r\n",
                receiver.ToString());
        }

        [TestMethod]
        public void ToStringIgnoredLineTest2()
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            receiver.AddOutput("Hello, World!");
            receiver.AddOutput("$See you!");

            receiver.Flush();

            Assert.AreEqual("Hello, World!\r\n",
                receiver.ToString());
        }

        [TestMethod]
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

            try
            {
                receiver.ThrowOnError(line);
                throw new AssertFailedException($"An exception of type {typeof(T).FullName} was not thrown");
            }
            catch (T)
            {
                // All OK - an exception of type T was thrown
            }
            catch (Exception ex)
            {
                throw new AssertFailedException($"An exception of type {typeof(T).FullName} was expected, but of type {ex.GetType().FullName} was thrown instead.");
            }
        }
    }
}
