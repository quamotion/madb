using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	internal class CommandErrorReceiver : MultiLineReceiver {
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandErrorReceiver" /> class.
		/// </summary>
		public CommandErrorReceiver ( ) {
			ErrorMessage = null;
		}

		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected override void ProcessNewLines ( string[] lines ) {
			StringBuilder message = new StringBuilder ( );
			foreach ( var line in lines ) {
				if ( String.IsNullOrEmpty ( line ) || line.StartsWith ( "#" ) || line.StartsWith("$") ) {
					continue;
				}

				message.AppendLine ( line );
			}

			if ( message.Length > 0 ) {
				ErrorMessage = message.ToString ( );
			}
		}

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		/// <value>
		/// The error message.
		/// </value>
		public String ErrorMessage { get; set; }
	}
}
