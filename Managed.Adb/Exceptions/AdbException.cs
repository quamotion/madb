using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Managed.Adb.Exceptions {
	/// <summary>
	/// Represents an exception with communicating with ADB
	/// </summary>
	public class AdbException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="AdbException"/> class.
		/// </summary>
		public AdbException( )
			: base ( "An error occurred with ADB" ) {

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdbException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public AdbException( String message )
			: base ( message ) {

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdbException"/> class.
		/// </summary>
		/// <param name="serializationInfo">The serialization info.</param>
		/// <param name="context">The context.</param>
		public AdbException( SerializationInfo serializationInfo,StreamingContext context) : base(serializationInfo,context) {

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdbException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public AdbException( String message, Exception innerException )
			: base ( message, innerException ) {

		}
	}
}
