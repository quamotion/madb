using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Exceptions {
	/// <summary>
	/// 
	/// </summary>
	public class BadPacketException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="BadPacketException"/> class.
		/// </summary>
		public BadPacketException ( )
			: base ( ) {


		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BadPacketException"/> class.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		public BadPacketException ( String msg )
			: base ( msg ) {

		}
	}
}
