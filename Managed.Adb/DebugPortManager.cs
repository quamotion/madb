using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// Centralized point to provide a IDebugPortProvider to ddmlib.
	/// </summary>
	public class DebugPortManager {
		public const int NO_STATIC_PORT = -1;

		public DebugPortManager ( ) {

		}

		/// <summary>
		/// Gets or sets the IDebugPortProvider that will be used when a new Client requests
		/// </summary>
		public IDebugPortProvider Provider { get; set; }

		private static DebugPortManager _instance;
		/// <summary>
		/// Returns an instance of the debug port manager
		/// </summary>
		public DebugPortManager Instance {
			get {
				if ( _instance == null ) {
					_instance = new DebugPortManager ( );
				}
				return _instance;
			}
		}
	}
}
