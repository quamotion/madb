namespace Managed.Adb.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ShellCommandUnresponsiveException : AdbException
    {
        public ShellCommandUnresponsiveException() : base("The shell command has become unresponsive")
        {
        }
    }
}
