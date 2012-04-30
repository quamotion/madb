using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Extensions {
	public static class IntegerHelper {

		// reverse byte order (16-bit)
		/// <summary>
		/// reverse byte order (16-bit)
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static UInt16 ReverseBytes ( this UInt16 value ) {
			return (UInt16)( ( value & 0xFFU ) << 8 | ( value & 0xFF00U ) >> 8 );
		}

		// reverse byte order (32-bit)
		/// <summary>
		/// reverse byte order (32-bit)
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static UInt32 ReverseBytes ( this UInt32 value ) {
			return ( value & 0x000000FFU ) << 24 | ( value & 0x0000FF00U ) << 8 |
						 ( value & 0x00FF0000U ) >> 8 | ( value & 0xFF000000U ) >> 24;
		}

		/// <summary>
		/// reverse byte order (32-bit)
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static Int32 ReverseBytes ( this Int32 value ) {
			return BitConverter.ToInt32 ( ReverseByteArray ( BitConverter.GetBytes ( value ) ), 0 );
		}

		/// <summary>
		/// reverse byte order (16-bit)
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static Int16 ReverseBytes ( this Int16 value ) {
			return BitConverter.ToInt16 ( ReverseByteArray ( BitConverter.GetBytes ( value ) ), 0 );
		}

		/// <summary>
		/// reverse byte order (64-bit)
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static Int64 ReverseBytes ( this Int64 value ) {
			return BitConverter.ToInt16 ( ReverseByteArray ( BitConverter.GetBytes ( value ) ), 0 );
		}

		// reverse byte order (64-bit)
		/// <summary>
		/// reverse byte order (64-bit)
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static UInt64 ReverseBytes ( this UInt64 value ) {
			return ( value & 0x00000000000000FFUL ) << 56 | ( value & 0x000000000000FF00UL ) << 40 |
						 ( value & 0x0000000000FF0000UL ) << 24 | ( value & 0x00000000FF000000UL ) << 8 |
						 ( value & 0x000000FF00000000UL ) >> 8 | ( value & 0x0000FF0000000000UL ) >> 24 |
						 ( value & 0x00FF000000000000UL ) >> 40 | ( value & 0xFF00000000000000UL ) >> 56;
		}

		public static byte[] ReverseByteArray ( this byte[] inArray ) {
			byte temp;
			int highCtr = inArray.Length - 1;

			for ( int ctr = 0; ctr < inArray.Length / 2; ctr++ ) {
				temp = inArray[ctr];
				inArray[ctr] = inArray[highCtr];
				inArray[highCtr] = temp;
				highCtr -= 1;
			}
			return inArray;
		}
	}
}
