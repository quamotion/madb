using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Managed.Adb.Extensions {
	public static class DateTimeExtensions {
		/// <summary>
		/// Converts a DateTime to Unix Epoch
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public static long ToUnixEpoch ( this DateTime date ) {
			TimeSpan t = ( date - new DateTime ( 1970, 1, 1 ) );
			long epoch = (long)t.TotalSeconds;
			return epoch;
		}

		/// <summary>
		/// Creates a DateTime from the seconds since Epoch
		/// </summary>
		/// <param name="seconds">The seconds.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromUnixEpoch ( this int seconds ) {
			DateTime epoch = new DateTime ( 1970, 1, 1 );
			DateTime ret = epoch.Add ( new TimeSpan ( 0, 0, seconds ) );
			return ret;
		}

		/// <summary>
		/// Creates a DateTime from a string
		/// </summary>
		/// <param name="dateString">The date string.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromString ( this String dateString ) {
			return DateTime.Parse ( dateString );
		}

		/// <summary>
		/// Creates a DateTime from a string
		/// </summary>
		/// <param name="dateString">The date string.</param>
		/// <param name="format">The format.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromString ( this String dateString, IFormatProvider format ) {
			return DateTime.Parse ( dateString, format );
		}

		/// <summary>
		/// Creates a DateTime from a string
		/// </summary>
		/// <param name="dateString">The date string.</param>
		/// <param name="format">The format.</param>
		/// <param name="styles">The styles.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromString ( this String dateString, IFormatProvider format, DateTimeStyles styles ) {
			return DateTime.Parse ( dateString, format, styles );
		}

		/// <summary>
		/// Creates a DateTime from a binary value
		/// </summary>
		/// <param name="dateData">The date data.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromBinary ( this long dateData ) {
			return DateTime.FromBinary ( dateData );
		}

		/// <summary>
		/// Creates a DateTime from a file time
		/// </summary>
		/// <param name="fileTime">The file time.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromFileTime ( this long fileTime ) {
			return DateTime.FromFileTime ( fileTime );
		}

		/// <summary>
		/// Creates a DateTime from a file time UTC
		/// </summary>
		/// <param name="fileTime">The file time.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromFileTimeUtc ( this long fileTime ) {
			return DateTime.FromFileTimeUtc ( fileTime );
		}

		/// <summary>
		/// Creates a DateTime from an OA Date
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public static DateTime ToDateTimeFromOADate ( this double date ) {
			return DateTime.FromOADate ( date );
		}
	}
}
