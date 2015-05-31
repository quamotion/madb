using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public sealed class EnvironmentVariablesReceiver : MultiLineReceiver {
		/// <summary>
		/// 
		/// </summary>
		public const String ENV_COMMAND = "printenv";
		/// <summary>
		/// 
		/// </summary>
		private const String ENV_PATTERN = @"^([^=\s]+)\s*=\s*(.*)$";
		/// <summary>
		/// Initializes a new instance of the <see cref="EnvironmentVariablesReceiver"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public EnvironmentVariablesReceiver ( Device device ) {
			Device = device;
		}

		/// <summary>
		/// Gets or sets the device.
		/// </summary>
		/// <value>The device.</value>
		public Device Device { get; private set; }

		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected override void ProcessNewLines ( string[] lines ) {
			foreach ( String line in lines ) {
				if ( String.IsNullOrEmpty ( line ) || line.StartsWith ( "#" ) ) {
					continue;
				}

				Match m = Regex.Match ( line, ENV_PATTERN );
				if ( m.Success ) {
					String label = m.Groups[1].Value.Trim ( );
					String value = m.Groups[2].Value.Trim ( );

					if ( label.Length > 0 ) {
						if ( Device.EnvironmentVariables.ContainsKey ( label ) ) {
							Device.EnvironmentVariables[label] = value;
						} else {
							Device.EnvironmentVariables.Add ( label, value );
						}
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
