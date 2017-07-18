using SharpAdbClient.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SharpAdbClient
{
    public class SocketBasedTests
    {
        protected static readonly AdbResponse[] NoResponses = new AdbResponse[] { };
        protected static readonly AdbResponse[] OkResponse = new AdbResponse[] { AdbResponse.OK };
        protected static readonly string[] NoResponseMessages = new string[] { };
        protected static readonly DeviceData Device = new DeviceData()
        {
            Serial = "169.254.109.177:5555",
            State = DeviceState.Online
        };

        protected IDummyAdbSocket Socket
        {
            get;
            set;
        }

        public EndPoint EndPoint
        {
            get;
            set;
        }

        public bool IntegrationTest
        {
            get;
            set;
        }

        protected void Initialize(bool integrationTest, bool doDispose)
        {
            this.EndPoint = AdbClient.Instance.EndPoint;

#if DEBUG
            // Use the tracing adb socket factory to run the tests on an actual device.
            // use the dummy socket factory to run unit tests.
            if (integrationTest)
            {
                var tracingSocket = new TracingAdbSocket(this.EndPoint) { DoDispose = doDispose };

                Factories.AdbSocketFactory = (endPoint) => tracingSocket;
            }
            else
            {
                var socket = new DummyAdbSocket();
                Factories.AdbSocketFactory = (endPoint) => socket;
            }

            this.IntegrationTest = integrationTest;
#else
            // In release mode (e.g. on the build server),
            // never run integration tests.
            var socket = new DummyAdbSocket();
            Factories.AdbSocketFactory = (endPoint) => socket;
            this.IntegrationTest = false;
#endif
            this.Socket = (IDummyAdbSocket)Factories.AdbSocketFactory(this.EndPoint);

            AdbClient.Instance = new AdbClient();
        }

        /// <summary>
        /// <para>
        /// Runs an ADB helper test, either as a unit test or as an integration test.
        /// </para>
        /// <para>
        /// When running as a unit test, the <paramref name="responses"/> and <paramref name="responseMessages"/>
        /// are used by the <see cref="DummyAdbSocket"/> to mock the responses an actual device
        /// would send; and the <paramref name="requests"/> parameter is used to ensure the code
        /// did send the correct requests to the device.
        /// </para>
        /// <para>
        /// When running as an integration test, all three parameters, <paramref name="responses"/>,
        /// <paramref name="responseMessages"/> and <paramref name="requests"/> are used to validate
        /// that the traffic we simulate in the unit tests matches the trafic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">
        /// The <see cref="AdbResponse"/> messages that the ADB sever should send.
        /// </param>
        /// <param name="responseMessages">
        /// The messages that should follow the <paramref name="responses"/>.
        /// </param>
        /// <param name="requests">
        /// The requests the client should send.
        /// </param>
        /// <param name="test">
        /// The test to run.
        /// </param>
        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Action test)
        {
            RunTest(responses, responseMessages, requests, null, null, null, null, null, test);
        }

        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Stream shellStream,
            Action test)
        {
            RunTest(responses, responseMessages, requests, null, null, null, null, shellStream, test);
        }

        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<Tuple<SyncCommand, string>> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            Action test)
        {
            this.RunTest(
                responses,
                responseMessages,
                requests,
                syncRequests,
                syncResponses,
                syncDataReceived,
                syncDataSent,
                null,
                test);
        }

        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<Tuple<SyncCommand, string>> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            Stream shellStream,
            Action test)
        {
            // If we are running unit tests, we need to mock all the responses
            // that are sent by the device. Do that now.
            if (!this.IntegrationTest)
            {
                this.Socket.ShellStream = shellStream;

                foreach (var response in responses)
                {
                    this.Socket.Responses.Enqueue(response);
                }

                foreach (var responseMessage in responseMessages)
                {
                    this.Socket.ResponseMessages.Enqueue(responseMessage);
                }

                if (syncResponses != null)
                {
                    foreach (var syncResponse in syncResponses)
                    {
                        this.Socket.SyncResponses.Enqueue(syncResponse);
                    }
                }

                if (syncDataReceived != null)
                {
                    foreach (var syncDatum in syncDataReceived)
                    {
                        this.Socket.SyncDataReceived.Enqueue(syncDatum);
                    }
                }
            }

            Exception exception = null;

            try
            {
                test();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (!this.IntegrationTest)
            {
                // If we are running unit tests, we need to make sure all messages
                // were read, and the correct request was sent.

                // Make sure the messages were read
                Assert.AreEqual(0, this.Socket.ResponseMessages.Count);
                Assert.AreEqual(0, this.Socket.Responses.Count);
                Assert.AreEqual(0, this.Socket.SyncResponses.Count);
                Assert.AreEqual(0, this.Socket.SyncDataReceived.Count);

                // Make sure a request was sent
                CollectionAssert.AreEqual(requests.ToList(), this.Socket.Requests);

                if (syncRequests != null)
                {
                    CollectionAssert.AreEqual(syncRequests.ToList(), this.Socket.SyncRequests);
                }
                else
                {
                    Assert.AreEqual(0, this.Socket.SyncRequests.Count);
                }

                if (syncDataSent != null)
                {
                    AssertEqual(syncDataSent.ToList(), this.Socket.SyncDataSent.ToList());
                }
                else
                {
                    Assert.AreEqual(0, this.Socket.SyncDataSent.Count);
                }
            }
            else
            {
                // Make sure the traffic sent on the wire matches the traffic
                // we have defined in our unit test.
                CollectionAssert.AreEqual(requests.ToList(), this.Socket.Requests);

                if (syncRequests != null)
                {
                    CollectionAssert.AreEqual(syncRequests.ToList(), this.Socket.SyncRequests);
                }
                else
                {
                    Assert.AreEqual(0, this.Socket.SyncRequests.Count);
                }

                CollectionAssert.AreEqual(responses.ToList(), this.Socket.Responses);
                CollectionAssert.AreEqual(responseMessages.ToList(), this.Socket.ResponseMessages);

                if (syncResponses != null)
                {
                    CollectionAssert.AreEqual(syncResponses.ToList(), this.Socket.SyncResponses);
                }
                else
                {
                    Assert.AreEqual(0, this.Socket.SyncResponses.Count);
                }

                if (syncDataReceived != null)
                {
                    AssertEqual(syncDataReceived.ToList(), this.Socket.SyncDataReceived.ToList());
                }
                else
                {
                    Assert.AreEqual(0, this.Socket.SyncDataReceived.Count);
                }

                if (syncDataSent != null)
                {
                    AssertEqual(syncDataSent.ToList(), this.Socket.SyncDataSent.ToList());
                }
                else
                {
                    Assert.AreEqual(0, this.Socket.SyncDataSent.Count);
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        protected static IEnumerable<string> Requests(params string[] requests)
        {
            return requests;
        }

        protected static IEnumerable<string> ResponseMessages(params string[] requests)
        {
            return requests;
        }

        protected static IEnumerable<Tuple<SyncCommand, string>> SyncRequests(SyncCommand command, string path)
        {
            yield return new Tuple<SyncCommand, string>(command, path);
        }

        protected static IEnumerable<Tuple<SyncCommand, string>> SyncRequests(SyncCommand command, string path, SyncCommand command2, string path2)
        {
            yield return new Tuple<SyncCommand, string>(command, path);
            yield return new Tuple<SyncCommand, string>(command2, path2);
        }

        protected static IEnumerable<Tuple<SyncCommand, string>> SyncRequests(SyncCommand command, string path, SyncCommand command2, string path2, SyncCommand command3, string path3)
        {
            yield return new Tuple<SyncCommand, string>(command, path);
            yield return new Tuple<SyncCommand, string>(command2, path2);
            yield return new Tuple<SyncCommand, string>(command3, path3);
        }

        protected static IEnumerable<AdbResponse> OkResponses(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return AdbResponse.OK;
            }
        }

        private void AssertEqual(IList<byte[]> expected, IList<byte[]> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
