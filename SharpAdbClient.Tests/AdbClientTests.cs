using SharpAdbClient.Exceptions;
using SharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class AdbClientTests : SocketBasedTests
    {
        // Toggle the integration test flag to true to run on an actual adb server
        // (and to build/validate the test cases), set to false to use the mocked
        // adb sockets.
        // In release mode, this flag is ignored and the mocked adb sockets are always used.
        public AdbClientTests()
            : base(integrationTest: false, doDispose: false)
        {
            Factories.Reset();
        }

        [Fact]
        public void ConstructorNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new AdbClient(null, Factories.AdbSocketFactory));
        }

        [Fact]
        public void ConstructorInvalidEndPointTest()
        {
            Assert.Throws<NotSupportedException>(() => new AdbClient(new CustomEndPoint(), Factories.AdbSocketFactory));
        }

        [Fact]
        public void ConstructorTest()
        {
            var adbClient = new AdbClient();
            Assert.NotNull(adbClient);
            Assert.NotNull(adbClient.EndPoint);
            Assert.IsType<IPEndPoint>(adbClient.EndPoint);

            var endPoint = (IPEndPoint)adbClient.EndPoint;

            Assert.Equal(IPAddress.Loopback, endPoint.Address);
            Assert.Equal(AdbClient.AdbServerPort, endPoint.Port);
        }

        [Fact]
        public void FormAdbRequestTest()
        {
            Assert.Equal(Encoding.ASCII.GetBytes("0009host:kill"), AdbClient.FormAdbRequest("host:kill"));
            Assert.Equal(Encoding.ASCII.GetBytes("000Chost:version"), AdbClient.FormAdbRequest("host:version"));
        }

        [Fact]
        public void CreateAdbForwardRequestTest()
        {
            Assert.Equal(Encoding.ASCII.GetBytes("0008tcp:1984"), AdbClient.CreateAdbForwardRequest(null, 1984));
            Assert.Equal(Encoding.ASCII.GetBytes("0012tcp:1981:127.0.0.1"), AdbClient.CreateAdbForwardRequest("127.0.0.1", 1981));
        }

        [Fact]
        public void KillAdbTest()
        {
            var requests = new string[]
            {
                "host:kill"
            };

            this.RunTest(
                NoResponses,
                NoResponseMessages,
                requests,
                () =>
                {
                    this.TestClient.KillAdb();
                });
        }

        [Fact]
        public void GetAdbVersionTest()
        {
            var responseMessages = new string[]
            {
                "0020"
            };

            var requests = new string[]
            {
                "host:version"
            };

            int version = 0;

            this.RunTest(
                OkResponse,
                responseMessages,
                requests,
                () =>
                {
                    version = this.TestClient.GetAdbVersion();
                });

            // Make sure and the correct value is returned.
            Assert.Equal(32, version);
        }

        [Fact]
        public void GetDevicesTest()
        {
            var responseMessages = new string[]
            {
                "169.254.109.177:5555   device product:VS Emulator 5\" KitKat (4.4) XXHDPI Phone model:5__KitKat__4_4__XXHDPI_Phone device:donatello\n"
            };

            var requests = new string[]
            {
                "host:devices-l"
            };

            List<DeviceData> devices = null;

            this.RunTest(
                OkResponse,
                responseMessages,
                requests,
                () =>
                {
                    devices = this.TestClient.GetDevices();
                });

            // Make sure and the correct value is returned.
            Assert.NotNull(devices);
            Assert.Single(devices);

            var device = devices.Single();

            Assert.Equal("169.254.109.177:5555", device.Serial);
            Assert.Equal(DeviceState.Online, device.State);
            Assert.Equal("5__KitKat__4_4__XXHDPI_Phone", device.Model);
            Assert.Equal("donatello", device.Name);
        }

        [Fact]
        public void SetDeviceTest()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            this.RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () =>
                {
                    this.Socket.SetDevice(Device);
                });
        }

        [Fact]
        public void SetInvalidDeviceTest()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            Assert.Throws<DeviceNotFoundException>(() => this.RunTest(
                new AdbResponse[] { AdbResponse.FromError("device not found") },
                NoResponseMessages,
                requests,
                () =>
                {
                    this.Socket.SetDevice(Device);
                }));
        }

        [Fact]
        public void SetDeviceOtherException()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            Assert.Throws<AdbException>(() => this.RunTest(
                new AdbResponse[] { AdbResponse.FromError("Too many cats.") },
                NoResponseMessages,
                requests,
                () =>
                {
                    this.Socket.SetDevice(Device);
                }));
        }

        [Fact]
        public void RebootTest()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reboot:"
            };

            this.RunTest(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                NoResponseMessages,
                requests,
                () =>
                {
                    this.TestClient.Reboot(Device);
                });
        }

        [Fact]
        public void ExecuteRemoteCommandTest()
        {
            var device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            var responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK
            };

            var responseMessages = new string[] { };

            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            };

            byte[] streamData = Encoding.ASCII.GetBytes("Hello, World\r\n");
            MemoryStream shellStream = new MemoryStream(streamData);

            var receiver = new ConsoleOutputReceiver();

            this.RunTest(
                responses,
                responseMessages,
                requests,
                shellStream,
                () =>
                {
                    this.TestClient.ExecuteRemoteCommand("echo Hello, World", device, receiver);
                });

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ExecuteRemoteCommandUnresponsiveTest()
        {
            var device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            var responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK
            };

            var responseMessages = new string[] { };

            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            };

            var receiver = new ConsoleOutputReceiver();

            Assert.Throws<ShellCommandUnresponsiveException>(() => this.RunTest(
                responses,
                responseMessages,
                requests,
                null,
                () =>
                {
                    this.TestClient.ExecuteRemoteCommand("echo Hello, World", device, receiver);
                }));
        }

        [Fact]
        public void CreateForwardTest()
        {
            this.RunCreateForwardTest(
                (device) => this.TestClient.CreateForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");
        }


        [Fact]
        public void CreateReverseTest()
        {
            this.RunCreateReverseTest(
                (device) => this.TestClient.CreateReverseForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");
        }

        [Fact]
        public void CreateTcpForwardTest()
        {
            this.RunCreateForwardTest(
                (device) => this.TestClient.CreateForward(device, 3, 4),
                "tcp:3;tcp:4");
        }

        [Fact]
        public void CreateSocketForwardTest()
        {
            this.RunCreateForwardTest(
                (device) => this.TestClient.CreateForward(device, 5, "/socket/1"),
                "tcp:5;local:/socket/1");
        }

        [Fact]
        public void CreateDuplicateForwardTest()
        {
            var responses = new AdbResponse[]
            {
                AdbResponse.FromError("cannot rebind existing socket")
            };

            var requests = new string[]
            {
                "host-serial:169.254.109.177:5555:forward:norebind:tcp:1;tcp:2"
            };

            Assert.Throws<AdbException>(() => this.RunTest(
                responses,
                NoResponseMessages,
                requests,
                () =>
                {
                    this.TestClient.CreateForward(Device, "tcp:1", "tcp:2", false);
                }));
        }

        [Fact]
        public void ListForwardTest()
        {
            var responseMessages = new string[] {
                "169.254.109.177:5555 tcp:1 tcp:2\n169.254.109.177:5555 tcp:3 tcp:4\n169.254.109.177:5555 tcp:5 local:/socket/1\n"
            };

            var requests = new string[]
            {
                "host-serial:169.254.109.177:5555:list-forward"
            };

            ForwardData[] forwards = null;

            this.RunTest(
                OkResponse,
                responseMessages,
                requests,
                () => forwards = this.TestClient.ListForward(Device).ToArray());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("169.254.109.177:5555", forwards[0].SerialNumber);
            Assert.Equal("tcp:1", forwards[0].Local);
            Assert.Equal("tcp:2", forwards[0].Remote);
        }

        [Fact]
        public void ListReverseForwardTest()
        {
            var responseMessages = new string[] {
                "(reverse) localabstract:scrcpy tcp:100\n(reverse) localabstract: scrcpy2 tcp:100\n(reverse) localabstract: scrcpy3 tcp:100\n"
            };
            var responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK,
            };

            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reverse:list-forward"
            };

            ForwardData[] forwards = null;

            this.RunTest(
                responses,
                responseMessages,
                requests,
                () => forwards = this.TestClient.ListReverseForward(Device).ToArray());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("(reverse)", forwards[0].SerialNumber);
            Assert.Equal("localabstract:scrcpy", forwards[0].Local);
            Assert.Equal("tcp:100", forwards[0].Remote);
        }

        [Fact]
        public void RemoveForwardTest()
        {
            var requests = new string[]
            {
                "host-serial:169.254.109.177:5555:killforward:tcp:1"
            };

            this.RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => this.TestClient.RemoveForward(Device, 1));
        }

        [Fact]
        public void RemoveReverseForwardTest()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reverse:killforward:localabstract:test"
            };

            var responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK,
            };

            this.RunTest(
                responses,
                NoResponseMessages,
                requests,
                () => this.TestClient.RemoveReverseForward(Device, "localabstract:test"));
        }

        [Fact]
        public void RemoveAllForwardsTest()
        {
            var requests = new string[]
            {
                "host-serial:169.254.109.177:5555:killforward-all"
            };

            this.RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => this.TestClient.RemoveAllForwards(Device));
        }

        [Fact]
        public void RemoveAllReversesTest()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reverse:killforward-all"
            };

            var responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK,
            };

            this.RunTest(
                responses,
                NoResponseMessages,
                requests,
                () => this.TestClient.RemoveAllReverseForwards(Device));
        }

        [Fact]
        public void ConnectIPAddressTest()
        {
            this.RunConnectTest(
                () => this.TestClient.Connect(IPAddress.Loopback),
                "127.0.0.1:5555");
        }

        [Fact]
        public void ConnectDnsEndpointTest()
        {
            this.RunConnectTest(
                () => this.TestClient.Connect(new DnsEndPoint("localhost", 1234)),
                "localhost:1234");
        }

        [Fact]
        public void ConnectIPEndpointTest()
        {
            this.RunConnectTest(
                () => this.TestClient.Connect(new IPEndPoint(IPAddress.Loopback, 4321)),
                "127.0.0.1:4321");
        }

        [Fact]
        public void ConnectHostEndpointTest()
        {
            this.RunConnectTest(
                () => this.TestClient.Connect("localhost"),
                "localhost:5555");
        }

        [Fact]
        public void ConnectIPAddressNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => this.TestClient.Connect((IPAddress)null));
        }

        [Fact]
        public void ConnectDnsEndpointNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => this.TestClient.Connect((DnsEndPoint)null));
        }

        [Fact]
        public void ConnectIPEndpointNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => this.TestClient.Connect((IPEndPoint)null));
        }

        [Fact]
        public void ConnectHostEndpointNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => this.TestClient.Connect((string)null));
        }

        [Fact]
        public void DisconnectTest()
        {
            var requests = new string[] { "host:disconnect:localhost:5555" };

            this.RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => this.TestClient.Disconnect(new DnsEndPoint("localhost", 5555)));
        }

        [Fact]
        public void ReadLogTest()
        {
            var device = new DeviceData()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            var responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK
            };

            var responseMessages = new string[] { };

            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            };

            var receiver = new ConsoleOutputReceiver();

            using (Stream stream = File.OpenRead("logcat.bin"))
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                Collection<Logs.LogEntry> logs = new Collection<LogEntry>();
                Action<LogEntry> sink = (entry) => logs.Add(entry);

                this.RunTest(
                    responses,
                    responseMessages,
                    requests,
                    shellStream,
                    () =>
                    {
                        this.TestClient.RunLogServiceAsync(device, sink, CancellationToken.None, Logs.LogId.System).Wait();
                    });

                Assert.Equal(3, logs.Count());
            }
        }

        [Fact]
        public void RootTest()
        {
            var device = new DeviceData()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            var requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "root:"
            };

            byte[] expectedData = new byte[1024];
            byte[] expectedString = Encoding.UTF8.GetBytes("adbd cannot run as root in production builds\n");
            Buffer.BlockCopy(expectedString, 0, expectedData, 0, expectedString.Length);

            Assert.Throws<AdbException>(() => this.RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                new Tuple<SyncCommand, string>[] { },
                new SyncCommand[] { },
                new byte[][] { expectedData },
                new byte[][] { },
                () => this.TestClient.Root(device)));
        }

        [Fact]
        public void UnrootTest()
        {
            var device = new DeviceData()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            var requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "unroot:"
            };

            byte[] expectedData = new byte[1024];
            byte[] expectedString = Encoding.UTF8.GetBytes("adbd not running as root\n");
            Buffer.BlockCopy(expectedString, 0, expectedData, 0, expectedString.Length);

            Assert.Throws<AdbException>(() => this.RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                new Tuple<SyncCommand, string>[] { },
                new SyncCommand[] { },
                new byte[][] { expectedData },
                new byte[][] { },
                () => this.TestClient.Unroot(device)));
        }

        [Fact]
        public void InstallTest()
        {
            var device = new DeviceData()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            var requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "exec:cmd package 'install'  -S 205774"
            };

            // The app data is sent in chunks of 32 kb
            Collection<byte[]> applicationDataChuncks = new Collection<byte[]>();

            using (Stream stream = File.OpenRead("testapp.apk"))
            {
                while (true)
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read = stream.Read(buffer, 0, buffer.Length);

                    if (read == 0)
                    {
                        break;
                    }
                    else
                    {
                        buffer = buffer.Take(read).ToArray();
                        applicationDataChuncks.Add(buffer);
                    }
                }
            }

            byte[] response = Encoding.UTF8.GetBytes("Success\n");

            using (Stream stream = File.OpenRead("testapp.apk"))
            {
                this.RunTest(
                    new AdbResponse[]
                    {
                        AdbResponse.OK,
                        AdbResponse.OK,
                    },
                    NoResponseMessages,
                    requests,
                    new Tuple<SyncCommand, string>[] { },
                    new SyncCommand[] { },
                    new byte[][] { response },
                    applicationDataChuncks.ToArray(),
                        () => this.TestClient.Install(device, stream));
            }
        }

        private void RunConnectTest(Action test, string connectString)
        {
            var requests = new string[]
            {
                $"host:connect:{connectString}"
            };

            this.RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                test);
        }

        private void RunCreateReverseTest(Action<DeviceData> test, string reverseString)
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                $"reverse:forward:{reverseString}",
            };

            this.RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                    AdbResponse.OK
                },
                new string[]
                {
                    null
                },
                requests,
                () => test(Device));
        }

        private void RunCreateForwardTest(Action<DeviceData> test, string forwardString)
        {
            var requests = new string[]
            {
                $"host-serial:169.254.109.177:5555:forward:{forwardString}"
            };

            this.RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK
                },
                new string[]
                {
                    null
                },
                requests,
                () => test(Device));
        }
    }
}
