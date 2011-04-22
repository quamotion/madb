using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class ConsoleOutputReceiver : MultiLineReceiver {
		private static ConsoleOutputReceiver _instance = null;
		public static ConsoleOutputReceiver Instance {
			get {
				if ( _instance == null ) {
					_instance = new ConsoleOutputReceiver ( );
				}
				return _instance;
			}
		}
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
