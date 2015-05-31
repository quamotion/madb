using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Exceptions {

	/// <summary>
	/// Exception thrown when adb refuses a command.
	/// </summary>
	public class AdbCommandRejectedException : AdbException {
		public AdbCommandRejectedException ( String message )
			: base ( message ) {
			IsDeviceOffline = message.Equals ( "device offline" );
			WasErrorDuringDeviceSelection = false;
		}
		public AdbCommandRejectedException ( String message, bool errorDuringDeviceSelection )
			: base ( message ) {
			WasErrorDuringDeviceSelection = errorDuringDeviceSelection;
			IsDeviceOffline = message.Equals ( "device offline" );
		}
		public bool IsDeviceOffline { get; private set; }
		public bool WasErrorDuringDeviceSelection { get; private set; }
	}
}
