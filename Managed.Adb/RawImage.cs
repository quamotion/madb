using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Managed.Adb.Conversion;

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
			// https://github.com/android/platform_system_core/blob/master/adb/framebuffer_service.c
			switch ( version ) {
				case 1: /* RGBA_8888 */
				case 2: /* RGBX_8888 */
				case 3: /* RGB_888 */
				case 4: /* RGB_565 */
				case 5: /* BGRA_8888 */
					this.Bpp = (int)buf.ReadInt32 ( );
					this.Size = (int)buf.ReadInt32 ( );
					this.Width = (int)buf.ReadInt32 ( ); // 480
					this.Height = (int)buf.ReadInt32 ( ); // 800
					this.Red.Offset = (int)buf.ReadInt32 ( ); // 8
					this.Red.Length = (int)buf.ReadInt32 ( ); // 8
					this.Blue.Offset = (int)buf.ReadInt32 ( );  // 0
					this.Blue.Length = (int)buf.ReadInt32 ( ); // 8
					this.Green.Offset = (int)buf.ReadInt32 ( ); // 16
					this.Green.Length = (int)buf.ReadInt32 ( ); // 8
					this.Alpha.Offset = (int)buf.ReadInt32 ( ); // 24
					this.Alpha.Length = (int)buf.ReadInt32 ( ); // 8
					break;
				default:
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
					break;
			}
			return true;
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
				case 2:
				case 3:
				case 4:
				case 5:
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
			rotated.Green.Offset = this.Green.Offset;
			rotated.Green.Length = this.Green.Length;
			rotated.Blue.Offset = this.Blue.Offset;
			rotated.Blue.Length = this.Blue.Length;
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
				}
			}

			return rotated;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString ( ) {
			return String.Format ( "height: {0}\nwidth: {1}\nbpp: {2}\nro: {3}\nrl: {4}\ngo: {5}\ngl: {6}\nbo: {7}\nbl: {8}\nao: {9}\nal: {10}\ns: {11}",
				this.Height, this.Width, this.Bpp,
				this.Red.Offset, this.Red.Length,
				this.Green.Offset, this.Green.Length,
				this.Blue.Offset, this.Blue.Length,
				this.Alpha.Offset, this.Alpha.Length, this.Size );
		}



		/// <summary>
		/// Converts this raw image to an Image
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns></returns>
		public Image ToImage ( PixelFormat format ) {
			Bitmap bitmap = null;
			Bitmap image = null;
			BitmapData bitmapdata = null;
			try {
				bitmap = new Bitmap ( this.Width, this.Height, format );
				bitmapdata = bitmap.LockBits ( new Rectangle ( 0, 0, this.Width, this.Height ), ImageLockMode.WriteOnly, format );
				image = new Bitmap ( this.Width, this.Height, format );
				var tdata = Data;
				if ( Bpp == 32 ) {
					tdata = Swap ( tdata );
				}
				Marshal.Copy ( tdata, 0, bitmapdata.Scan0, this.Size );
				bitmap.UnlockBits ( bitmapdata );
				using ( Graphics g = Graphics.FromImage ( image ) ) {
					g.DrawImage ( bitmap, new Point ( 0, 0 ) );
					return image;
				}

			} catch ( Exception ) {
				throw;
			}
		}

		/// <summary>
		/// Converts this raw image to an Image
		/// </summary>
		/// <returns></returns>
		public Image ToImage ( ) {
			return ToImage ( this.Bpp == 32 ? PixelFormat.Format32bppArgb : PixelFormat.Format16bppRgb565 );
		}

		private byte[] Swap ( byte[] b ) {
			var clone = new List<byte> ( );
			b.IntReverseForRawImage ( bitem => {
				clone.AddRange ( bitem );
			} );
			return clone.ToArray ( );
		}


	}
}
