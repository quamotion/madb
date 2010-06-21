using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Managed.Adb {
	public class RawImage {
		public int Version { get; set; }
		public int Bpp { get; set; }
		public int Size { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int RedOffset { get; set; }
		public int RedLength { get; set; }
		public int BlueOffset { get; set; }
		public int BlueLength { get; set; }
		public int GreenOffset { get; set; }
		public int GreenLength { get; set; }
		public int AlphaOffset { get; set; }
		public int AlphaLength { get; set; }

		public byte[] Data { get; set; }

		/**
		 * Reads the header of a RawImage from a {@link ByteBuffer}.
		 * <p/>The way the data is sent over adb is defined in system/core/adb/framebuffer_service.c
		 * @param version the version of the protocol.
		 * @param buf the buffer to read from.
		 * @return true if success
		 */
		public bool ReadHeader ( int version, BinaryReader buf ) {
			this.Version = version;

			if ( version == 16 ) {
				// compatibility mode with original protocol
				this.Bpp = 16;

				// read actual values.
				this.Size = buf.ReadInt32 ( );
				this.Width = buf.ReadInt32 ( );
				this.Height = buf.ReadInt32 ( );

				// create default values for the rest. Format is 565
				this.RedOffset = 11;
				this.RedLength = 5;
				this.GreenOffset = 5;
				this.GreenLength = 6;
				this.BlueOffset = 0;
				this.BlueLength = 5;
				this.AlphaOffset = 0;
				this.AlphaLength = 0;
			} else if ( version == 1 ) {
				this.Bpp = buf.ReadInt32 ( );
				this.Size = buf.ReadInt32 ( );
				this.Width = buf.ReadInt32 ( );
				this.Height = buf.ReadInt32 ( );
				this.RedOffset = buf.ReadInt32 ( );
				this.RedLength = buf.ReadInt32 ( );
				this.BlueOffset = buf.ReadInt32 ( );
				this.BlueLength = buf.ReadInt32 ( );
				this.GreenOffset = buf.ReadInt32 ( );
				this.GreenLength = buf.ReadInt32 ( );
				this.AlphaOffset = buf.ReadInt32 ( );
				this.AlphaLength = buf.ReadInt32 ( );
			} else {
				// unsupported protocol!
				return false;
			}

			return true;
		}

		/**
		 * Returns the mask value for the red color.
		 * <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		 */
		public int GetRedMask ( ) {
			return GetMask ( RedLength, RedOffset );
		}

		/**
		 * Returns the mask value for the green color.
		 * <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		 */
		public int GetGreenMask ( ) {
			return GetMask ( GreenLength, GreenOffset );
		}

		/**
		 * Returns the mask value for the blue color.
		 * <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		 */
		public int GetBlueMask ( ) {
			return GetMask ( BlueLength, BlueOffset );
		}

		/**
		 * Returns the size of the header for a specific version of the framebuffer adb protocol.
		 * @param version the version of the protocol
		 * @return the number of int that makes up the header.
		 */
		public static int GetHeaderSize ( int version ) {
			switch ( version ) {
				case 16: // compatibility mode
					return 3; // size, width, height
				case 1:
					return 12; // bpp, size, width, height, 4*(length, offset)
			}

			return 0;
		}

		/**
		 * Returns a rotated version of the image
		 * The image is rotated counter-clockwise.
		 */
		public RawImage GetRotated ( ) {
			RawImage rotated = new RawImage ( );
			rotated.Version = this.Version;
			rotated.Bpp = this.Bpp;
			rotated.Size = this.Size;
			rotated.RedOffset = this.RedOffset;
			rotated.RedLength = this.RedLength;
			rotated.BlueOffset = this.BlueOffset;
			rotated.BlueLength = this.BlueLength;
			rotated.GreenOffset = this.GreenOffset;
			rotated.GreenLength = this.GreenLength;
			rotated.AlphaOffset = this.AlphaOffset;
			rotated.AlphaLength = this.AlphaLength;

			rotated.Width = this.Height;
			rotated.Height = this.Width;

			int count = this.Data.Length;
			rotated.Data = new byte[count];

			int byteCount = this.Bpp >> 3; // bpp is in bits, we want bytes to match our array
			int w = this.Width;
			int h = this.Height;
			for ( int y = 0; y < h; y++ ) {
				for ( int x = 0; x < w; x++ ) {
					Array.Copy ( this.Data, ( y * w + x ) * byteCount,
						rotated.Data, ( ( w - x - 1 ) * h + y ) * byteCount,
										byteCount );
					/*System.arraycopy(
									this.data, (y * w + x) * byteCount,
									rotated.data, ((w-x-1) * h + y) * byteCount,
									byteCount);*/
				}
			}

			return rotated;
		}

		/**
		 * Returns an ARGB integer value for the pixel at <var>index</var> in {@link #data}.
		 */
		public int GetARGB ( int index ) {
			int value;
			if ( Bpp == 16 ) {
				value = Data[index] & 0x00FF;
				value |= ( Data[index + 1] << 8 ) & 0x0FF00;
			} else if ( Bpp == 32 ) {
				value = Data[index] & 0x00FF;
				value |= ( Data[index + 1] & 0x00FF ) << 8;
				value |= ( Data[index + 2] & 0x00FF ) << 16;
				value |= ( Data[index + 3] & 0x00FF ) << 24;
			} else {
				throw new ArgumentException ( "RawImage.getARGB(int) only works in 16 and 32 bit mode." );
			}

			int r = ( ( value >> RedOffset ) & GetMask ( RedLength ) ) << ( 8 - RedLength );
			int g = ( ( value >> GreenOffset ) & GetMask ( GreenLength ) ) << ( 8 - GreenLength );
			int b = ( ( value >> BlueOffset ) & GetMask ( BlueLength ) ) << ( 8 - BlueLength );
			int a;
			if ( AlphaLength == 0 ) {
				a = 0xFF; // force alpha to opaque if there's no alpha value in the framebuffer.
			} else {
				a = ( ( value >> AlphaOffset ) & GetMask ( AlphaLength ) ) << ( 8 - AlphaLength );
			}

			return a << 24 | r << 16 | g << 8 | b;
		}

		/**
		 * creates a mask value based on a length and offset.
		 * <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		 */
		private int GetMask ( int length, int offset ) {
			int res = GetMask ( length ) << offset;

			// if the bpp is 32 bits then we need to invert it because the buffer is in little endian
			if ( Bpp == 32 ) {
				return BitConverter.ToInt32 ( ReverseBytes ( BitConverter.GetBytes ( res ) ), 0 );
			}

			return res;
		}

		private static byte[] ReverseBytes ( byte[] inArray ) {
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


		/**
		 * Creates a mask value based on a length.
		 * @param length
		 * @return
		 */
		private static int GetMask ( int length ) {
			return ( 1 << length ) - 1;
		}
	}
}
