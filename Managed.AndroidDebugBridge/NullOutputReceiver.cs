using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public sealed class NullOutputReceiver : IShellOutputReceiver {

		private NullOutputReceiver ( ) {
			IsCancelled = false;
		}

		private static NullOutputReceiver _instance = null;

		public static IShellOutputReceiver Instance {
			get {
				if ( _instance == null ) {
					_instance = new NullOutputReceiver ( );
				}
				return _instance;
			}
		}

		public void AddOutput ( byte[] data, int offset, int length ) {
		}

		public void Flush ( ) {
		}

		public bool IsCancelled { get; private set; }
	}

}
