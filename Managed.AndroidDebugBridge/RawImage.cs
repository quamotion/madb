using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Managed.Adb.Extensions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace Managed.Adb {
	/// <summary>
	/// Data representing an image taken from a device frame buffer.
	/// </summary>
	public class RawImage {
		/// <summary>
		/// Initializes a new instance of the <see cref="RawImage"/> class.
		/// </summary>
		public RawImage ( ) {
			this.Red = new ColorData ( );
			this.Blue = new ColorData ( );
			this.Green = new ColorData ( );
			this.Alpha = new ColorData ( );
		}

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		public int Version { get; set; }
		/// <summary>
		/// Gets or sets the BPP.
		/// </summary>
		/// <value>
		/// The BPP.
		/// </value>
		public int Bpp { get; set; }
		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		/// <value>
		/// The size.
		/// </value>
		public int Size { get; set; }
		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>
		/// The width.
		/// </value>
		public int Width { get; set; }
		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>
		/// The height.
		/// </value>
		public int Height { get; set; }
		/// <summary>
		/// Gets or sets the red.
		/// </summary>
		/// <value>
		/// The red.
		/// </value>
		public ColorData Red { get; set; }
		/// <summary>
		/// Gets or sets the blue.
		/// </summary>
		/// <value>
		/// The blue.
		/// </value>
		public ColorData Blue { get; set; }
		/// <summary>
		/// Gets or sets the green.
		/// </summary>
		/// <value>
		/// The green.
		/// </value>
		public ColorData Green { get; set; }
		/// <summary>
		/// Gets or sets the alpha.
		/// </summary>
		/// <value>
		/// The alpha.
		/// </value>
		public ColorData Alpha { get; set; }

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>
		/// The data.
		/// </value>
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
			Console.WriteLine ( "version: {0}", version );
			if ( version == 16 ) {
				// compatibility mode with original protocol
				this.Bpp = 16;

				// read actual values.
				this.Size = buf.ReadInt32 ( );
				this.Width = buf.ReadInt32 ( );
				this.Height = buf.ReadInt32 ( );

				// create default values for the rest. Format is 565
				this.Red.Offset = 11;
				this.Red.Length = 5;
				this.Green.Offset = 5;
				this.Green.Length = 6;
				this.Blue.Offset = 0;
				this.Blue.Length = 5;
				this.Alpha.Offset = 0;
				this.Alpha.Length = 0;
			} else if ( version == 1 ) {
				this.Bpp = buf.ReadInt32 ( );
				this.Size = buf.ReadInt32 ( );
				this.Width = buf.ReadInt32 ( );
				this.Height = buf.ReadInt32 ( );
				this.Blue.Offset = buf.ReadInt32 ( );
				this.Blue.Length = buf.ReadInt32 ( );
				this.Green.Offset = buf.ReadInt32 ( );
				this.Green.Length = buf.ReadInt32 ( );
				this.Red.Offset = buf.ReadInt32 ( );
				this.Red.Length = buf.ReadInt32 ( );
				this.Alpha.Offset = buf.ReadInt32 ( );
				this.Alpha.Length = buf.ReadInt32 ( );

				Console.WriteLine ( this.ToString ( ) );
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
			return GetMask ( Red.Length, Red.Offset );
		}

		/**
		 * Returns the mask value for the green color.
		 * <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		 */
		public int GetGreenMask ( ) {
			return GetMask ( Green.Length, Green.Offset );
		}

		/**
		 * Returns the mask value for the blue color.
		 * <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		 */
		public int GetBlueMask ( ) {
			return GetMask ( Blue.Length, Blue.Offset );
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
			rotated.Red.Offset = this.Red.Offset;
			rotated.Red.Length = this.Red.Length;
			rotated.Blue.Offset = this.Blue.Offset;
			rotated.Blue.Length = this.Blue.Length;
			rotated.Green.Offset = this.Green.Offset;
			rotated.Green.Length = this.Green.Length;
			rotated.Alpha.Offset = this.Alpha.Offset;
			rotated.Alpha.Length = this.Alpha.Length;

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

			// based on (http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/6c892d10-75ff-4f4e-a555-3017f72ec170/)
			// casting as uint will perform a bit shift with zero fill, like >>> in java.
			int r = ( (int)( (uint)value >> Red.Offset ) & GetMask ( Red.Length ) ) << ( 8 - Red.Length );
			int g = ( (int)( (uint)value >> Green.Offset ) & GetMask ( Green.Length ) ) << ( 8 - Green.Length );
			int b = ( (int)( (uint)value >> Blue.Offset ) & GetMask ( Blue.Length ) ) << ( 8 - Blue.Length );
			int a;
			if ( Alpha.Length == 0 ) {
				a = 0xFF; // force alpha to opaque if there's no alpha value in the framebuffer.
			} else {
				a = ( (int)( (uint)value >> Alpha.Offset ) & GetMask ( Alpha.Length ) ) << ( 8 - Alpha.Length );
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
				//return BitConverter.ToInt32 ( ReverseBytes ( BitConverter.GetBytes ( res ) ), 0 );
				return res.ReverseBytes ( );
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

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString ( ) {
			return String.Format ( "height: {0}\nwidth: {1}\nbpp: {2}\nro: {3}\nrl: {4}\ngo: {5}\ngl: {6}\nbo: {7}\nbl: {8}\nao: {9}\nal: {10}\n",
				this.Height, this.Width, this.Bpp,
				this.Red.Offset, this.Red.Length,
				this.Green.Offset, this.Green.Length,
				this.Blue.Offset, this.Blue.Length,
				this.Alpha.Offset, this.Alpha.Length );
		}

		public Image ToImage ( PixelFormat format ) {
			int pixels = this.Size;
			Bitmap bitmap = null;
			Bitmap image = null;
			BitmapData bitmapdata = null;
			try {
				bitmap = new Bitmap ( this.Width, this.Height, format );
				bitmapdata = bitmap.LockBits ( new Rectangle ( 0, 0, this.Width, this.Height ), ImageLockMode.WriteOnly, format );
				image = new Bitmap ( this.Width, this.Height, format );

				for ( int i = 0; i < this.Data.Length; i++ ) {
					Marshal.WriteByte ( bitmapdata.Scan0, i, this.Data[i] );
				}
				bitmap.UnlockBits ( bitmapdata );



				using ( Graphics g = Graphics.FromImage ( image ) ) {
					g.DrawImage ( bitmap, new Point ( 0, 0 ) );
					return image;
				}

			} catch ( Exception ) {
				throw;
			}
		}
	}
}
