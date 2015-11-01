using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Managed.Adb.Tests
{
    internal class DummyAdbSocketFactory : IAdbSocketFactory
    {
        public DummyAdbSocketFactory()
        {
            this.Socket = new DummyAdbSocket();
        }

        public DummyAdbSocket Socket
        {
            get;
            private set;
        }

        public Exception Exception
        {
            get;
            set;
        }

        public IAdbSocket Create(IPEndPoint endPoint)
        {
            if(this.Exception != null)
            {
                throw this.Exception;
            }

            return this.Socket;
        }
    }
}
