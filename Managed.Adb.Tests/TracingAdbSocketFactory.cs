using System.Net;

namespace Managed.Adb.Tests
{
    internal class TracingAdbSocketFactory : IAdbSocketFactory
    {
        public TracingAdbSocketFactory()
        {
            this.Socket = new TracingAdbSocket(AndroidDebugBridge.SocketAddress);
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
