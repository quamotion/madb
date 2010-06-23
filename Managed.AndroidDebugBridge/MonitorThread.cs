using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	class MonitorThread {
		private MonitorThread ( ) {

		}
		private static MonitorThread _instance;
		public static MonitorThread Instance {
			get {
				if ( _instance == null ) {
					_instance = new MonitorThread ( );
				}
				return _instance;
			}
		}

		internal void SetDebugSelectedPort ( int value ) {
			//throw new NotImplementedException ( );
		}
	}
}
