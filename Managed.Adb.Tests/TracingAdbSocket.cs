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
        
        public void Read(byte[] data)
        {
            base.Read(data);
        }

        public AdbResponse ReadAdbResponse(bool readDiagString)
        {
            var response = base.ReadAdbResponse(readDiagString);
            this.Responses.Enqueue(response);
            return response;
        }

        public string ReadString()
        {
            var value = base.ReadString();
            this.ResponseMessages.Enqueue(value);
            return value;
        }

        public void SendAdbRequest(string request)
        {
            this.Requests.Add(request);
            base.SendAdbRequest(request);
        }
    }
}
