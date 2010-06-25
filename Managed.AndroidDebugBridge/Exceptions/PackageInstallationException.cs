using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Exceptions {
	[Serializable]
	public class PackageInstallationException : Exception {
		public PackageInstallationException ( ) { }
		public PackageInstallationException ( string message ) : base ( message ) { }
		public PackageInstallationException ( string message, Exception inner ) : base ( message, inner ) { }
		protected PackageInstallationException (
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context )
			: base ( info, context ) { }
	}
}
