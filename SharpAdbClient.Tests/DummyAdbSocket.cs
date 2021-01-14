using SharpAdbClient.Exceptions;
using Xunit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace SharpAdbClient.Tests
{
    internal class DummyAdbSocket : IAdbSocket, IDummyAdbSocket
    {
        /// <summary>
        /// Use this message to cause <see cref="ReadString"/> and <see cref="ReadStringAsync(CancellationToken)"/> to throw
        /// a <see cref="AdbException"/> indicating that the adb server has forcefully closed the connection.
        /// </summary>
        public const string ServerDisconnected = "ServerDisconnected";

        public DummyAdbSocket()
        {
            this.IsConnected = true;
        }

        public Stream ShellStream
        {
            get;
            set;
        }

        public Queue<AdbResponse> Responses
        {
            get;
        } = new Queue<AdbResponse>();

        public Queue<SyncCommand> SyncResponses
        {
            get;
        } = new Queue<SyncCommand>();

        public Queue<byte[]> SyncDataReceived
        {
            get;
        } = new Queue<byte[]>();

        public Queue<byte[]> SyncDataSent
        {
            get;
        } = new Queue<byte[]>();

        public Queue<string> ResponseMessages
        { get; } = new Queue<string>();

        public List<string> Requests
        { get; } = new List<string>();

        public List<Tuple<SyncCommand, string>> SyncRequests
        { get; } = new List<Tuple<SyncCommand, string>>();

        public bool IsConnected
        {
            get;
            set;
        }

        public bool WaitForNewData
        {
            get;
            set;
        }

        public bool Connected
        {
            get
            {
                return this.IsConnected
                    && (this.WaitForNewData || this.Responses.Count > 0 || this.ResponseMessages.Count > 0 || this.SyncResponses.Count > 0 || this.SyncDataReceived.Count > 0);
            }
        }

        /// <inheritdoc/>
        public bool DidReconnect
        {
            get;
            private set;
        }

        public Socket Socket
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            this.IsConnected = false;
        }

        public int Read(byte[] data)
        {
            var actual = this.SyncDataReceived.Dequeue();

            for (int i = 0; i < data.Length && i < actual.Length; i++)
            {
                data[i] = actual[i];
            }

            return actual.Length;
        }

        public Task ReadAsync(byte[] data, CancellationToken cancellationToken)
        {
            this.Read(data);

            return Task.FromResult(true);
        }

        public AdbResponse ReadAdbResponse()
        {
            var response = this.Responses.Dequeue();

            if (!response.Okay)
            {
                throw new AdbException(response.Message, response);
            }

            return response;
        }

        public string ReadString()
        {
            return this.ReadStringAsync(CancellationToken.None).Result;
        }

        public string ReadSyncString()
        {
            return this.ResponseMessages.Dequeue();
        }

        public async Task<string> ReadStringAsync(CancellationToken cancellationToken)
        {
            if (this.WaitForNewData)
            {
                while (this.ResponseMessages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            var message = this.ResponseMessages.Dequeue();

            if (message == ServerDisconnected)
            {
                var socketException = new SocketException(AdbServer.ConnectionReset);
                throw new AdbException(socketException.Message, socketException);
            }
            else
            {
                return message;
            }
        }

        public void SendAdbRequest(string request)
        {
            this.Requests.Add(request);
        }

        public void Close()
        {
            this.IsConnected = false;
        }

        public void SendSyncRequest(string command, int value)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, int length)
        {
            this.SyncDataSent.Enqueue(data.Take(length).ToArray());
        }

        public int Read(byte[] data, int length)
        {
            var actual = this.SyncDataReceived.Dequeue();

            Assert.Equal(actual.Length, length);

            Buffer.BlockCopy(actual, 0, data, 0, length);

            return actual.Length;
        }

        public void SendSyncRequest(SyncCommand command, string path)
        {
            this.SyncRequests.Add(new Tuple<SyncCommand, string>(command, path));
        }

        public SyncCommand ReadSyncResponse()
        {
            return this.SyncResponses.Dequeue();
        }

        public void SendSyncRequest(SyncCommand command, int length)
        {
            this.SyncRequests.Add(new Tuple<SyncCommand, string>(command, length.ToString()));
        }

        public void SendSyncRequest(SyncCommand command, string path, int permissions)
        {
            this.SyncRequests.Add(new Tuple<SyncCommand, string>(command, $"{path},{permissions}"));
        }

        public Stream GetShellStream()
        {
            if (this.ShellStream != null)
            {
                return this.ShellStream;
            }
            else
            {
                // Simulate the device failing to respond properly.
                throw new SocketException();
            }
        }

        public void Reconnect()
        {
            this.DidReconnect = true;
        }

        public Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, int offset, int length)
        {
            if (offset == 0)
            {
                this.Send(data, length);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetDevice(DeviceData device)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                this.SendAdbRequest($"host:transport:{device.Serial}");

                try
                {
                    var response = this.ReadAdbResponse();
                }
                catch (AdbException e)
                {
                    if (string.Equals("device not found", e.AdbError, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DeviceNotFoundException(device.Serial);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
