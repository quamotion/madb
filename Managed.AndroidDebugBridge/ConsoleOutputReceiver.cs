using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class ConsoleOutputReceiver : MultiLineReceiver {

			protected override void ProcessNewLines ( string[] lines ) {
				foreach ( var line in lines ) {
					if ( String.IsNullOrEmpty ( line ) || line.StartsWith ( "#" ) ) {
						continue;
					}
					Console.WriteLine ( line );
				}
			}
	}
}
