using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	/// <summary>
	/// String Helper Class
	/// </summary>
	public static partial class MadbExtensions {
		/// <summary>
		/// Formats the string with the specified arguments
		/// </summary>
		/// <param name="s">The string.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>The newly formatted string.</returns>
		public static String With ( this String s, params object[] args ) {
			return String.Format ( s, args );
		}

		public static String ToArgumentSafe ( this String s ) {
			return "{0}{1}{0}".With ( s.Contains ( " " ) ? "\"" : String.Empty, s );
		}


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

		/// <summary>
		/// Determines whether [is null or white space] [the specified source].
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns>
		///   <c>true</c> if [is null or white space] [the specified source]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNullOrWhiteSpace( this String source ) {
			return String.IsNullOrEmpty ( source ) || String.IsNullOrEmpty ( source.Trim ( ) );
		}

		/// <summary>
		/// Matches the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="pattern">The pattern.</param>
		/// <returns></returns>
		public static Match Match ( this String source, String pattern ) {
			return Match ( source, pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase );
		}

		/// <summary>
		/// Matches the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="options">The options.</param>
		/// <returns></returns>
		public static Match Match ( this String source, String pattern, RegexOptions options ) {
			return Regex.Match ( source, pattern, options );
		}

		/// <summary>
		/// Determines whether the specified source is match.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="pattern">The pattern.</param>
		/// <returns>
		///   <c>true</c> if the specified source is match; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsMatch ( this String source, String pattern ) {
			return IsMatch ( source, pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase );
		}

		/// <summary>
		/// Determines whether the specified source is match.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="options">The options.</param>
		/// <returns>
		///   <c>true</c> if the specified source is match; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsMatch ( this String source, String pattern, RegexOptions options ) {
			return Regex.IsMatch ( source, pattern, options );
		}
	}
}
