using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Exceptions {
	[global::System.Serializable]
	public class UnsupportedEncodingException : Exception {
		public UnsupportedEncodingException ( ) { }
		public UnsupportedEncodingException ( string message ) : base ( message ) { }
		public UnsupportedEncodingException ( string message, Exception inner ) : base ( message, inner ) { }
		protected UnsupportedEncodingException (
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context )
			: base ( info, context ) { }
	}
}
