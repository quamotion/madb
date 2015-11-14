using System.Net;

namespace SharpAdbClient.Tests
{
    internal class TracingAdbSocketFactory : IAdbSocketFactory
    {
        public TracingAdbSocketFactory(bool doDispose)
        {
            this.Socket = new TracingAdbSocket(AdbServer.EndPoint)
            {
                DoDispose = doDispose
            };
        }

        public IDummyAdbSocket Socket
        {
            get;
            private set;
        }

        public IAdbSocket Create(IPEndPoint endPoint)
        {
            return this.Socket;
        }
    }
}
