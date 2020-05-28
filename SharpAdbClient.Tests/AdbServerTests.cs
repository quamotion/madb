using Moq;
using SharpAdbClient.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class AdbServerTests
    {
        private Func<string, IAdbCommandLineClient> adbCommandLineClientFactory;
        private DummyAdbSocket socket;
        private DummyAdbCommandLineClient commandLineClient;
        private Func<EndPoint, IAdbSocket> adbSocketFactory;
        private AdbClient adbClient;
        private AdbServer adbServer;

        public AdbServerTests()
        {
            this.socket = new DummyAdbSocket();
            this.adbSocketFactory = (endPoint) => this.socket;

            this.commandLineClient = new DummyAdbCommandLineClient();
            this.adbCommandLineClientFactory = (version) => this.commandLineClient;

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);
        }

        [Fact]
        public void GetStatusNotRunningTest()
        {
            var adbClientMock = new Mock<IAdbClient>();
            adbClientMock.Setup(c => c.GetAdbVersion())
                .Throws(new SocketException(AdbServer.ConnectionRefused));

            var adbServer = new AdbServer(adbClientMock.Object, this.adbCommandLineClientFactory);

            var status = adbServer.GetStatus();
            Assert.False(status.IsRunning);
            Assert.Null(status.Version);
        }

        [Fact]
        public void GetStatusRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0020");

            var status = this.adbServer.GetStatus();

            Assert.Empty(this.socket.Responses);
            Assert.Empty(this.socket.ResponseMessages);
            Assert.Single(this.socket.Requests);
            Assert.Equal("host:version", this.socket.Requests[0]);

            Assert.True(status.IsRunning);
            Assert.Equal(new Version(1, 0, 32), status.Version);
        }

        [Fact]
        public void GetStatusOtherSocketExceptionTest()
        {
            this.adbSocketFactory = (endPoint) =>
            {
                throw new SocketException();
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            Assert.Throws<SocketException>(() => this.adbServer.GetStatus());
        }

        [Fact]
        public void GetStatusOtherExceptionTest()
        {
            this.adbSocketFactory = (endPoint) =>
            {
                throw new Exception();
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            Assert.Throws<Exception>(() => this.adbServer.GetStatus());
        }

        [Fact]
        public void StartServerAlreadyRunningTest()
        {
            this.commandLineClient.Version = new Version(1, 0, 20);
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0020");

            var result = this.adbServer.StartServer(null, false);

            Assert.Equal(StartServerResult.AlreadyRunning, result);

            Assert.Single(this.socket.Requests);
            Assert.Equal("host:version", this.socket.Requests[0]);
        }

        [Fact]
        public void StartServerOutdatedRunningNoExecutableTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0010");

            Assert.Throws<AdbException>(() => this.adbServer.StartServer(null, false));
        }

        [Fact]
        public void StartServerNotRunningNoExecutableTest()
        {
            this.adbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            Assert.Throws<AdbException>(() => this.adbServer.StartServer(null, false));
        }

        [Fact]
        public void StartServerOutdatedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("0010");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(this.commandLineClient.ServerStarted);

            var result = this.adbServer.StartServer(this.ServerName, false);

            Assert.True(this.commandLineClient.ServerStarted);

            Assert.Equal(2, this.socket.Requests.Count);
            Assert.Equal("host:version", this.socket.Requests[0]);
            Assert.Equal("host:kill", this.socket.Requests[1]);
        }

        [Fact]
        public void StartServerNotRunningTest()
        {
            this.adbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            this.adbClient = new AdbClient(AdbClient.DefaultEndPoint, this.adbSocketFactory);
            this.adbServer = new AdbServer(this.adbClient, this.adbCommandLineClientFactory);

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(this.commandLineClient.ServerStarted);

            var result = this.adbServer.StartServer(this.ServerName, false);

            Assert.True(this.commandLineClient.ServerStarted);
        }

        [Fact]
        public void StartServerIntermediateRestartRequestedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("001f");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(this.commandLineClient.ServerStarted);

            var result = this.adbServer.StartServer(this.ServerName, true);

            Assert.True(this.commandLineClient.ServerStarted);

            Assert.Equal(2, this.socket.Requests.Count);
            Assert.Equal("host:version", this.socket.Requests[0]);
            Assert.Equal("host:kill", this.socket.Requests[1]);
        }

        [Fact]
        public void StartServerIntermediateRestartNotRequestedRunningTest()
        {
            this.socket.Responses.Enqueue(AdbResponse.OK);
            this.socket.ResponseMessages.Enqueue("001f");

            this.commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(this.commandLineClient.ServerStarted);

            var result = this.adbServer.StartServer(this.ServerName, false);

            Assert.False(this.commandLineClient.ServerStarted);

            Assert.Single(this.socket.Requests);
            Assert.Equal("host:version", this.socket.Requests[0]);
        }

        [Fact]
        public void ConstructorAdbClientNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new AdbServer(null, this.adbCommandLineClientFactory));
        }

        private string ServerName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "adb.exe" : "adb";
    }
}
