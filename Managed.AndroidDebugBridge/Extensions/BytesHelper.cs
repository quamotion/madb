using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public static partial class ManagedAdbExtenstions {

		/// <summary>
		/// Reverses the bytes.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static UInt16 ReverseBytes ( this UInt16 value ) {
			return (UInt16)( ( value & 0xFFU ) << 8 | ( value & 0xFF00U ) >> 8 );
		}

		/// <summary>
		/// Reverses the bytes.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static UInt32 ReverseBytes ( this UInt32 value ) {
			return ( value & 0x000000FFU ) << 24 | ( value & 0x0000FF00U ) << 8 |
				 ( value & 0x00FF0000U ) >> 8 | ( value & 0xFF000000U ) >> 24;
		}

		/// <summary>
		/// Reverses the bytes.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static UInt64 ReverseBytes ( this UInt64 value ) {
			return ( value & 0x00000000000000FFUL ) << 56 | ( value & 0x000000000000FF00UL ) << 40 |
				 ( value & 0x0000000000FF0000UL ) << 24 | ( value & 0x00000000FF000000UL ) << 8 |
				 ( value & 0x000000FF00000000UL ) >> 8 | ( value & 0x0000FF0000000000UL ) >> 24 |
				 ( value & 0x00FF000000000000UL ) >> 40 | ( value & 0xFF00000000000000UL ) >> 56;
		}


		/// <summary>
		/// Ints the reverse for raw image.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="action">The action.</param>
		public static void IntReverseForRawImage ( this byte[] source, Action<byte[]> action ) {
			var step = 4;
			for ( int i = 0; i < source.Count ( ); i += step ) {
				var b = new byte[step];
				for ( int x = b.Length - 1; x >= 0; --x ) {
					b[( step - 1 ) - x] = source[i + x];
				}

				b[2] = source[i + 0];
				b[1] = source[i + 1];
				b[0] = source[i + 2];
				b[3] = source[i + 3];

				action ( b );
			}
		}
	}
}
