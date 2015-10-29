using System;
using System.Collections;
using System.Collections.Generic;

namespace Managed.Adb.Tests
{
    internal class DummyAdbSocket : IAdbSocket, IDummyAdbSocket
    {
        public Queue<AdbResponse> Responses
        {
            get;
        } = new Queue<AdbResponse>();

        public Queue<string> ResponseMessages
        { get; } = new Queue<string>();

        public List<string> Requests
        { get; } = new List<string>();

        public void Dispose()
        {
        }

        public void Read(byte[] data)
        {
        }

        public AdbResponse ReadAdbResponse(bool readDiagString)
        {
            return this.Responses.Dequeue();
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
    }
}
