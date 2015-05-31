using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Exceptions {
	/// <summary>
	/// Unable to connect to the device because it was not found in the list of available devices.
	/// </summary>
	public class DeviceNotFoundException : AdbException {
		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceNotFoundException"/> class.
		/// </summary>
		public DeviceNotFoundException( ) : base("The device was not found.") {
				
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceNotFoundException"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public DeviceNotFoundException( String device )
			: base ( "The device '" + device + "' was not found." ) {

		}
	}
}
