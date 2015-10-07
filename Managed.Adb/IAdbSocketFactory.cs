using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Managed.Adb
{
    public interface IAdbSocketFactory
    {
        IAdbSocket Create(IPEndPoint endPoint);
    }
}
