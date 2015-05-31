using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Exceptions {
	public class ShellCommandUnresponsiveException : AdbException {
		public ShellCommandUnresponsiveException ( ) : base("The shell command has become unresponsive"){

		}
	}
}
