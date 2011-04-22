using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Exceptions {
	public class DeviceNotFoundException : AdbException {
		public DeviceNotFoundException( ) : base("The device was not found.") {
				
		}

		public DeviceNotFoundException( String device )
			: base ( "The device '" + device + "' was not found." ) {

		}
	}
}
