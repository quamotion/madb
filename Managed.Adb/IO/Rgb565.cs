using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

namespace Managed.Adb.IO {
	/// <summary>
	/// A class that represents Rgb565 Image information
	/// </summary>
	public static class Rgb565 {

		/// <summary>
		/// Gets the Image from the raw image data
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="data">The data.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <returns></returns>
		public static Image ToImage( PixelFormat format, byte[] data, int width, int height ) {
			int pixels = data.Length / 2;
			Bitmap bitmap = null;
			Bitmap image = null;
			BitmapData bitmapdata = null;
			try {
				bitmap = new Bitmap ( width, height, format );
				bitmapdata = bitmap.LockBits ( new Rectangle ( 0, 0, width, height ), ImageLockMode.WriteOnly, format );
				image = new Bitmap ( width, height, format );

				for ( int i = 0; i < data.Length; i++ ) {
					Marshal.WriteByte ( bitmapdata.Scan0, i, data[i] );
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

		/// <summary>
		/// Converts the data to an image
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <returns></returns>
		public static Image ToImage( byte[] data, int width, int height ) {
			return ToImage ( PixelFormat.Format16bppRgb565, data, width, height );
		}

		/// <summary>
		/// Saves the image as an Rgb565 bitmap to disk
		/// </summary>
		/// <param name="image">The image.</param>
		/// <param name="file">The file.</param>
		/// <returns></returns>
		public static bool ToRgb565( this Image image, string file ) {
			try {
				Bitmap bmp = image as Bitmap;
				BitmapData bmpData = bmp.LockBits ( new Rectangle ( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadOnly, PixelFormat.Format16bppRgb565 );
				using ( FileStream fs = new FileStream ( file, FileMode.Create, FileAccess.Write, FileShare.Read ) ) {
					byte[] buffer = new byte[307200];
					Marshal.Copy ( bmpData.Scan0, buffer, 0, buffer.Length );
					fs.Write ( buffer, 0, buffer.Length );
				}
				bmp.UnlockBits ( bmpData );
				return true;
			} catch {
				return false;
			}
		}

		/// <summary>
		/// Saves the image as an Rgb565 bitmap to the specified stream
		/// </summary>
		/// <param name="image">The image.</param>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		public static bool ToRgb565( this Image image, Stream stream ) {
			try {
				Bitmap bmp = image as Bitmap;
				BitmapData bmpData = bmp.LockBits ( new Rectangle ( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadOnly, PixelFormat.Format16bppRgb565 );
				using ( stream ) {
					byte[] buffer = new byte[307200];
					Marshal.Copy ( bmpData.Scan0, buffer, 0, buffer.Length );
					stream.Write ( buffer, 0, buffer.Length );
				}
				bmp.UnlockBits ( bmpData );
				return true;
			} catch {
				return false;
			}
		}
	}

}
