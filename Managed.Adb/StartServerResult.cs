using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb
{
    public enum StartServerResult
    {
        AlreadyRunning,
        RestartedOutdatedDaemon,
        Started
    }
}
