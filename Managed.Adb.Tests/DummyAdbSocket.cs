using Managed.Adb.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Managed.Adb.Tests
{
    internal class DummyAdbSocket : IAdbSocket, IDummyAdbSocket
    {
        public DummyAdbSocket()
        {
            this.Connected = true;
        }

        public Queue<AdbResponse> Responses
        {
            get;
        } = new Queue<AdbResponse>();

        public Queue<string> ResponseMessages
        { get; } = new Queue<string>();

        public List<string> Requests
        { get; } = new List<string>();

        public bool Connected
        {
            get;
            set;
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
            this.Connected = false;
        }

        public void Read(byte[] data)
        {
        }

        public AdbResponse ReadAdbResponse(bool readDiagString)
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
            return this.ResponseMessages.Dequeue();
        }

        public void SendAdbRequest(string request)
        {
            this.Requests.Add(request);
        }

        public int Read(byte[] data, int timeout)
        {
            return 0;
        }

        public void Close()
        {
            this.Connected = false;
        }

        public void SendFileRequest(string command, string path, SyncService.FileMode mode)
        {
            throw new NotImplementedException();
        }

        public void SendSyncRequest(string command, int value)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, int length, int timeout)
        {
            throw new NotImplementedException();
        }

        public void Read(byte[] data, int length, int timeout)
        {
            throw new NotImplementedException();
        }
    }
}
