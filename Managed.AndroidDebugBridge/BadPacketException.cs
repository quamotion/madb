using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class BadPacketException : Exception {
		public BadPacketException ( )
			: base ( ) {


		}

		public BadPacketException ( String msg )
			: base ( msg ) {

		}
	}
}
