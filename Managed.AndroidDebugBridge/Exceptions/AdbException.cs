using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Managed.Adb.Exceptions {
	public class AdbException : Exception {
		public AdbException( )
			: base ( "An error occurred with ADB" ) {

		}

		public AdbException( String message )
			: base ( message ) {

		}

		public AdbException( SerializationInfo serializationInfo,StreamingContext context) : base(serializationInfo,context) {

		}

		public AdbException( String message, Exception innerException )
			: base ( message, innerException ) {

		}
	}
}
