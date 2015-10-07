using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Managed.Adb
{
    public class AdbSocketFactory : IAdbSocketFactory
    {
        public IAdbSocket Create(IPEndPoint endPoint)
        {
            return new AdbSocket(endPoint);
        }
    }
}
