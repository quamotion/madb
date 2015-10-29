using Managed.Adb.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;

namespace Managed.Adb.Tests
{
    internal class TracingAdbSocket : AdbSocket, IDummyAdbSocket
    {
        public TracingAdbSocket(IPEndPoint endPoint) : base(endPoint)
        {
        }

        public Queue<AdbResponse> Responses
        {
            get;
        } = new Queue<AdbResponse>();

        public Queue<string> ResponseMessages
        { get; } = new Queue<string>();

        public List<string> Requests
        { get; } = new List<string>();

        public override void Dispose()
        {
            // Don't dispose the underlying socket. The tests always re-use
            // the same socket.
        }

        public void Read(byte[] data)
        {
            base.Read(data);
        }

        public override AdbResponse ReadAdbResponse(bool readDiagString)
        {
            Exception exception = null;
            AdbResponse response;

            try
            {
                response = base.ReadAdbResponse(readDiagString);
            }
            catch(AdbException ex)
            {
                exception = ex;
                response = ex.Response;
            }

            this.Responses.Enqueue(response);

            if(exception != null)
            {
                throw exception;
            }

            return response;
        }

        public override string ReadString()
        {
            var value = base.ReadString();
            this.ResponseMessages.Enqueue(value);
            return value;
        }

        public override void SendAdbRequest(string request)
        {
            this.Requests.Add(request);
            base.SendAdbRequest(request);
        }
    }
}
