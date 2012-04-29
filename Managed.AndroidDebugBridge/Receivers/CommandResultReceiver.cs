using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class CommandResultReceiver : MultiLineReceiver{
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

		public String Result { get; private set; }
	}
}
