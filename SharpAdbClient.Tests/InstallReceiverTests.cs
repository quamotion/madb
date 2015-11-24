using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class InstallReceiverTests
    {
        [TestMethod]
        public void ProcessFailureTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Failure [message]");
            receiver.Flush();

            Assert.IsFalse(receiver.Success);
            Assert.AreEqual("message", receiver.ErrorMessage);
        }

        [TestMethod]
        public void ProcessFailureEmptyMessageTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Failure [  ]");
            receiver.Flush();

            Assert.IsFalse(receiver.Success);
            Assert.AreEqual(InstallReceiver.UnknownError, receiver.ErrorMessage);
        }

        [TestMethod]
        public void ProcessFailureNoMessageTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Failure");
            receiver.Flush();

            Assert.IsFalse(receiver.Success);
            Assert.AreEqual(InstallReceiver.UnknownError, receiver.ErrorMessage);
        }

        [TestMethod]
        public void ProcessSuccessTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Success");
            receiver.Flush();

            Assert.IsTrue(receiver.Success);
        }
    }
}
