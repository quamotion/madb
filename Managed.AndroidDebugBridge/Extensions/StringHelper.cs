using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System {
	/// <summary>
	/// String Helper Class
	/// </summary>
	public static class StringHelper {
		/// <summary>
		/// Gets the bytes from a string.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns></returns>
		public static byte[] GetBytes ( this String str ) {
			return GetBytes ( str, Encoding.Default );
		}

		/// <summary>
		/// Gets the bytes from a string.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static byte[] GetBytes ( this String str, Encoding encoding ) {
			return encoding.GetBytes ( str );
		}

		/// <summary>
		/// Gets the bytes from a string.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static byte[] GetBytes ( this String str, String encoding ) {
			Encoding enc = Encoding.GetEncoding ( encoding );

			return GetBytes ( str, enc );
		}

		/// <summary>
		/// Converts a byte to the Hex value
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public static String ToHex ( this byte b ) {
			StringBuilder hex = new StringBuilder ( 2 );
			hex.AppendFormat ( "{0:x2}", b );
			return hex.ToString ( );
		}

		/// <summary>
		/// Converts a byte to the Hex value
		/// </summary>
		/// <param name="byteArray">The byte array.</param>
		/// <returns></returns>
		public static String ToHex ( this byte[] byteArray ) {
			StringBuilder hex = new StringBuilder ( byteArray.Length * 2 );
			foreach ( byte b in byteArray ) {
				hex.AppendFormat ( "{0} ", b.ToHex ( ) );
			}
			return hex.ToString ( ).Trim();

		}

		/// <summary>
		/// Gets the string from a byte array.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		public static String GetString ( this byte[] bytes ) {
			return GetString ( bytes, Encoding.Default );
		}

		/// <summary>
		/// Gets the string from a byte array.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static String GetString ( this byte[] bytes, Encoding encoding ) {
			return encoding.GetString ( bytes, 0, bytes.Length );
		}

		/// <summary>
		/// Gets the string from a byte array.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static String GetString ( this byte[] bytes, String encoding ) {
			Encoding enc = Encoding.GetEncoding ( encoding );
			return GetString ( bytes, enc );
		}

		/// <summary>
		/// Gets the string from a byte array.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		public static String GetString ( this byte[] bytes, int index, int count ) {
			return GetString ( bytes, index, count, Encoding.Default );
		}

		/// <summary>
		/// Gets the string from a byte array.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static String GetString ( this byte[] bytes, int index, int count, Encoding encoding ) {
			return encoding.GetString ( bytes, index, count );
		}

		/// <summary>
		/// Gets the string from a byte array.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static String GetString ( this byte[] bytes, int index, int count, String encoding ) {
			Encoding enc = Encoding.GetEncoding ( encoding );
			return GetString ( bytes, index, count, enc );
		}
	}
}
