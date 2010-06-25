using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Managed.Adb {
	class MonitorThread {

		public enum ThreadState {
			UNKNOWN = -1,
			Ready = 2,
			Disconnected = 3,
		}

		private MonitorThread ( ) {
			Clients = new List<IClient> ( );
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

		public bool Quit { get; private set; }
		public List<IClient> Clients { get; private set; }

		internal void SetDebugSelectedPort ( int value ) {
			//throw new NotImplementedException ( );
		}
	}
}
