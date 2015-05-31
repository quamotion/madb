using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public sealed class GetPropReceiver : MultiLineReceiver {
		/// <summary>
		/// The getprop command
		/// </summary>
		public const String GETPROP_COMMAND = "getprop";
		private const String GETPROP_PATTERN = "^\\[([^]]+)\\]\\:\\s*\\[(.*)\\]$";

		/// <summary>
		/// Initializes a new instance of the <see cref="GetPropReceiver"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public GetPropReceiver ( Device device ) {
			this.Device = device;
		}

		/// <summary>
		/// Gets or sets the device.
		/// </summary>
		/// <value>The device.</value>
		public Device Device { get; set; }


		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected override void ProcessNewLines ( string[] lines ) {
			// We receive an array of lines. We're expecting
			// to have the build info in the first line, and the build
			// date in the 2nd line. There seems to be an empty line
			// after all that.

			foreach ( String line in lines ) {
				if ( String.IsNullOrEmpty ( line ) || line.StartsWith ( "#" ) || line.StartsWith("$") ) {
					continue;
				}
				var m = line.Match ( GETPROP_PATTERN, RegexOptions.Compiled );
				if ( m.Success ) {
					String label = m.Groups[1].Value.Trim ( );
					String value = m.Groups[2].Value.Trim ( );

					if ( label.Length > 0 ) {
						Device.Properties.Add ( label, value );
					}
				}
			}
		}

		/// <summary>
		/// Finishes the receiver
		/// </summary>
		protected override void Done ( ) {
			this.Device.OnBuildInfoChanged ( EventArgs.Empty );
			base.Done ( );
		}
	}
}
