using SharpAdbClient.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Drawing.Imaging;
using System.IO;
using SharpAdbClient.Logs;
using System.Threading;
using System.Collections.ObjectModel;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbClientTests : SocketBasedTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest()
        {
            new AdbClient(null, Factories.AdbSocketFactory);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConstructorInvalidEndPointTest()
        {
            new AdbClient(new CustomEndPoint(), Factories.AdbSocketFactory);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var adbClient = new AdbClient();
            Assert.IsNotNull(adbClient);
            Assert.IsNotNull(adbClient.EndPoint);
            Assert.IsInstanceOfType(adbClient.EndPoint, typeof(IPEndPoint));

            var endPoint = (IPEndPoint)adbClient.EndPoint;

            Assert.AreEqual(IPAddress.Loopback, endPoint.Address);
            Assert.AreEqual(AdbClient.AdbServerPort, endPoint.Port);
        }

        [TestMethod]
        public void FormAdbRequestTest()
        {
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0009host:kill\n"), AdbClient.FormAdbRequest("host:kill"));
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("000Chost:version\n"), AdbClient.FormAdbRequest("host:version"));
        }

        [TestMethod]
        public void CreateAdbForwardRequestTest()
        {
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0008tcp:1984\n"), AdbClient.CreateAdbForwardRequest(null, 1984));
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("0012tcp:1981:127.0.0.1\n"), AdbClient.CreateAdbForwardRequest("127.0.0.1", 1981));
        }

        [TestInitialize]
        public void Initialize()
        {
            Factories.Reset();
            // Toggle the integration test flag to true to run on an actual adb server
            // (and to build/validate the test cases), set to false to use the mocked
            // adb sockets.
            // In release mode, this flag is ignored and the mocked adb sockets are always used.
            base.Initialize(integrationTest: false, doDispose: false);
        }

        [TestMethod]
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
                    AdbClient.Instance.KillAdb();
                });
        }

        [TestMethod]
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
                    version = AdbClient.Instance.GetAdbVersion();
                });

            // Make sure and the correct value is returned.
            Assert.AreEqual(32, version);
        }

        [TestMethod]
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
                    devices = AdbClient.Instance.GetDevices();
                });

            // Make sure and the correct value is returned.
            Assert.IsNotNull(devices);
            Assert.AreEqual(1, devices.Count);

            var device = devices.Single();

            Assert.AreEqual("169.254.109.177:5555", device.Serial);
            Assert.AreEqual(DeviceState.Online, device.State);
            Assert.AreEqual("5__KitKat__4_4__XXHDPI_Phone", device.Model);
            Assert.AreEqual("donatello", device.Name);
        }

        [TestMethod]
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
                    AdbClient.Instance.SetDevice(this.Socket, Device);
                });
        }

        [TestMethod]
        [ExpectedException(typeof(DeviceNotFoundException))]
        public void SetInvalidDeviceTest()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            this.RunTest(
                new AdbResponse[] { AdbResponse.FromError("device not found") },
                NoResponseMessages,
                requests,
                () =>
                {
                    AdbClient.Instance.SetDevice(this.Socket, Device);
                });
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
        public void SetDeviceOtherException()
        {
            var requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            this.RunTest(
                new AdbResponse[] { AdbResponse.FromError("Too many cats.") },
                NoResponseMessages,
                requests,
                () =>
                {
                    AdbClient.Instance.SetDevice(this.Socket, Device);
                });
        }

        [TestMethod]
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
                    AdbClient.Instance.Reboot(Device);
                });
        }

        [TestMethod]
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
                    AdbClient.Instance.ExecuteRemoteCommand("echo Hello, World", device, receiver);
                });

            Assert.AreEqual("Hello, World\r\n", receiver.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ShellCommandUnresponsiveException))]
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

            this.RunTest(
                responses,
                responseMessages,
                requests,
                null,
                () =>
                {
                    AdbClient.Instance.ExecuteRemoteCommand("echo Hello, World", device, receiver);
                });
        }

        [TestMethod]
        public void CreateForwardTest()
        {
            this.RunCreateForwardTest(
                (device) => AdbClient.Instance.CreateForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");
        }

        [TestMethod]
        public void CreateTcpForwardTest()
        {
            this.RunCreateForwardTest(
                (device) => AdbClient.Instance.CreateForward(device, 3, 4),
                "tcp:3;tcp:4");
        }

        [TestMethod]
        public void CreateSocketForwardTest()
        {
            this.RunCreateForwardTest(
                (device) => AdbClient.Instance.CreateForward(device, 5, "/socket/1"),
                "tcp:5;local:/socket/1");
        }

        [TestMethod]
        [ExpectedException(typeof(AdbException))]
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

            this.RunTest(
                responses,
                NoResponseMessages,
                requests,
                () =>
                {
                    AdbClient.Instance.CreateForward(Device, "tcp:1", "tcp:2", false);
                });
        }

        [TestMethod]
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
                () => forwards = AdbClient.Instance.ListForward(Device).ToArray());

            Assert.IsNotNull(forwards);
            Assert.AreEqual(3, forwards.Length);
            Assert.AreEqual("169.254.109.177:5555", forwards[0].SerialNumber);
            Assert.AreEqual("tcp:1", forwards[0].Local);
            Assert.AreEqual("tcp:2", forwards[0].Remote);
        }

        [TestMethod]
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
                () => AdbClient.Instance.RemoveForward(Device, 1));
        }

        [TestMethod]
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
                () => AdbClient.Instance.RemoveAllForwards(Device));
        }

        [TestMethod]
        public void ConnectIPAddressTest()
        {
            this.RunConnectTest(
                () => AdbClient.Instance.Connect(IPAddress.Loopback),
                "127.0.0.1:5555");
        }

        [TestMethod]
        public void ConnectDnsEndpointTest()
        {
            this.RunConnectTest(
                () => AdbClient.Instance.Connect(new DnsEndPoint("localhost", 1234)),
                "localhost:1234");
        }

        [TestMethod]
        public void ConnectIPEndpointTest()
        {
            this.RunConnectTest(
                () => AdbClient.Instance.Connect(new IPEndPoint(IPAddress.Loopback, 4321)),
                "127.0.0.1:4321");
        }

        [TestMethod]
        public void ConnectHostEndpointTest()
        {
            this.RunConnectTest(
                () => AdbClient.Instance.Connect("localhost"),
                "localhost:5555");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectIPAddressNullTest()
        {
            AdbClient.Instance.Connect((IPAddress)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectDnsEndpointNullTest()
        {
            AdbClient.Instance.Connect((DnsEndPoint)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectIPEndpointNullTest()
        {
            AdbClient.Instance.Connect((IPEndPoint)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectHostEndpointNullTest()
        {
            AdbClient.Instance.Connect((string)null);
        }

        [TestMethod]
        public void DisconnectTest()
        {
            var requests = new string[] { "host:disconnect:localhost:5555" };

            this.RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => AdbClient.Instance.Disconnect(new DnsEndPoint("localhost", 5555)));
        }

        [TestMethod]
        [DeploymentItem("logcat.bin")]
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
                        AdbClient.Instance.RunLogServiceAsync(device, sink, CancellationToken.None, Logs.LogId.System).Wait();
                    });

                Assert.AreEqual(3, logs.Count());
            }
        }

        [TestMethod]
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

            Assert.ThrowsException<AdbException>(() => this.RunTest(
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
                () => AdbClient.Instance.Root(device)));
        }

        [TestMethod]
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

            Assert.ThrowsException<AdbException>(() => this.RunTest(
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
                () => AdbClient.Instance.Unroot(device)));
        }

        [TestMethod]
        [DeploymentItem("testapp.apk")]
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
                        () => AdbClient.Instance.Install(device, stream));
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

        private void RunCreateForwardTest(Action<DeviceData> test, string forwardString)
        {
            var requests = new string[]
            {
                $"host-serial:169.254.109.177:5555:forward:{forwardString}"
            };

            this.RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => test(Device));
        }
    }
}
