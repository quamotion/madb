using Xunit;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    public class InstallReceiverTests
    {
        [Fact]
        public void ProcessFailureTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Failure [message]");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal("message", receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessFailureEmptyMessageTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Failure [  ]");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallReceiver.UnknownError, receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessFailureNoMessageTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Failure");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallReceiver.UnknownError, receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessSuccessTest()
        {
            InstallReceiver receiver = new InstallReceiver();
            receiver.AddOutput("Success");
            receiver.Flush();

            Assert.True(receiver.Success);
        }
    }
}
