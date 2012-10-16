using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.IO {
	public class ByteOrder {

		private ByteOrder ( String name ) {
			this.Name = name;
		}

		public readonly static ByteOrder BIG_ENDIAN = new ByteOrder ( "BIG_ENDIAN" );
		public readonly static ByteOrder LITTLE_ENDIAN = new ByteOrder ( "LITTLE_ENDIAN" );

		public String Name { get; private set; }
		public String ToString ( ) {
			return Name;
		}
	}

}
