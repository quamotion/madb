using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public class CommandResultReceiver : MultiLineReceiver{
		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected override void ProcessNewLines ( string[] lines ) {
			var result = new StringBuilder ( );
			foreach ( String line in lines ) {
				if ( String.IsNullOrEmpty ( line ) || line.StartsWith ( "#" ) || line.StartsWith ( "$" ) ) {
					continue;
				}

				result.AppendLine ( line );
			}

			this.Result = result.ToString ( ).Trim ( );
		}

		/// <summary>
		/// Gets the result.
		/// </summary>
		/// <value>
		/// The result.
		/// </value>
		public String Result { get; private set; }
	}
}
