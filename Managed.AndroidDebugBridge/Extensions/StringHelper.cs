using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Extensions {
	public static class StringHelper {
		public static byte[] GetBytes ( this String str ) {
			return GetBytes ( str, Encoding.Default );
		}

		public static byte[] GetBytes ( this String str, Encoding encoding ) {
			return encoding.GetBytes ( str );
		}

		public static byte[] GetBytes ( this String str, String encoding ) {
			Encoding enc = Encoding.GetEncoding ( encoding );

			return GetBytes ( str, enc );
		}

		public static String GetString ( this byte[] bytes ) {
			return GetString ( bytes, Encoding.Default );
		}

		public static String GetString ( this byte[] bytes, Encoding encoding ) {
			return encoding.GetString ( bytes, 0, bytes.Length );
		}

		public static String GetString ( this byte[] bytes, String encoding ) {
			Encoding enc = Encoding.GetEncoding ( encoding );
			return GetString ( bytes, enc );
		}

		public static String GetString ( this byte[] bytes, int index, int count ) {
			return GetString ( bytes, index, count, Encoding.Default );
		}

		public static String GetString ( this byte[] bytes, int index, int count, Encoding encoding ) {
			return encoding.GetString ( bytes, index, count );
		}

		public static String GetString ( this byte[] bytes, int index, int count, String encoding ) {
			Encoding enc = Encoding.GetEncoding ( encoding );
			return GetString ( bytes, index, count, enc );
		}
	}
}
