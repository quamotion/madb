#region Copyright
//  
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che 
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved. 
// 
// Modifications Copyright (C) 2002 Fang Yang. All rights reserved. 
//  
// This file is part of dicomcs, see http://www.sourceforge.net/projects/dicom-cs 
// 
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.                                  
//  
// This library is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU 
// Lesser General Public License for more details. 
//  
// You should have received a copy of the GNU Lesser General Public 
// License along with this library; if not, write to the Free Software 
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA 
// 
// Fang Yang (yangfang@email.com) 
// 
#endregion 

namespace Managed.Adb.IO {
	using System;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Summary description for ByteBuffer.
	/// </summary>
	/// <ignore>true</ignore>
	public class ByteBuffer : MemoryStream {
		private ByteOrder order = ByteOrder.LITTLE_ENDIAN;
		private BinaryReader reader = null;
		private BinaryWriter writer = null;

		public int Remaining {
			get { return (int)( Length - Position ); }
		}

		///////////////////////////////////////////////////////////////////////
		/// Constructor
		///////////////////////////////////////////////////////////////////////

		//private ByteBuffer( byte[] buf ) : this( buf, ByteOrder.LITTLE_ENDIAN )
		//{			
		//}

		public ByteBuffer ( byte[] buf, ByteOrder order )
			: base ( buf ) {
			SetOrder ( order );
		}

		public ByteBuffer ( int size, ByteOrder order )
			: base ( size ) {
			SetOrder ( order );
		}

		public ByteBuffer ( ) {
		}

		///////////////////////////////////////////////////////////////////////
		/// Public Instance Methods
		///////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Return length as int, o/w, we can use Length (long) directly
		/// </summary>
		/// <returns></returns>
		public int length ( ) {
			return (int)Length;
		}

		public ByteOrder GetOrder ( ) {
			return order;
		}

		public ByteBuffer SetOrder ( ByteOrder order ) {
			this.order = order;

			// Both reader and writer work on the same back store: MemoryStream
			if ( order == ByteOrder.LITTLE_ENDIAN ) {
				reader = new BinaryReader ( this );
				writer = new BinaryWriter ( this );
			} else {
				reader = new BEBinaryReader ( this );
				writer = new BEBinaryWriter ( this );
			}
			return this;
		}

		public ByteBuffer Rewind ( ) {
			Position = 0;
			return this;
		}

		public ByteBuffer Clear ( ) {
			Position = 0;
			SetLength ( 0 );
			return this;
		}

		/// <summary>
		/// Skip bytes
		/// </summary>
		/// <param name="count">How many bytes to skip</param>
		/// <returns>Actual bytes skipped</returns>
		public int Skip ( int count ) {
			int old = (int)Position;
			Position += count;
			if ( Position > Length )
				return (int)Length - old;
			return count;
		}

		/// <summary>
		/// Skip one byte
		/// </summary>
		/// <returns>Actual bytes skipped</returns>
		public int Skip ( ) {
			return Skip ( 1 );
		}


		/// <summary>
		/// ByteBuffer
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( ByteBuffer data ) {
			writer.Write ( data.ToArray ( ) );
			return this;
		}
		public virtual ByteBuffer ReadBuffer ( int len ) {
			reader.ReadBytes ( len );
			return this;
		}
		public virtual ByteBuffer ReadBuffer ( int offset, int len ) {
			Position = offset;
			reader.ReadBytes ( len );
			return this;
		}

		/// <summary>
		/// Byte
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( byte value ) {
			writer.Write ( value );
			return this;
		}
		public virtual byte ReadByte ( ) {
			return reader.ReadByte ( );
		}
		public virtual ByteBuffer Write ( byte value, int off ) {
			Position = off;
			writer.Write ( value );
			return this;
		}
		public virtual byte ReadByte ( int off ) {
			Position = off;
			return reader.ReadByte ( );
		}

		/// <summary>
		/// Bytes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( byte[] value ) {
			writer.Write ( value );
			return this;
		}

		/// <summary>
		/// Short
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( short value ) {
			writer.Write ( value );
			return this;
		}
		public virtual short ReadInt16 ( ) {
			return reader.ReadInt16 ( );
		}
		public virtual ByteBuffer Write ( int off, short value ) {
			Position = off;
			writer.Write ( value );
			return this;
		}
		public virtual short ReadInt16 ( int off ) {
			Position = off;
			return reader.ReadInt16 ( );
		}

		/// <summary>
		/// Int
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( int value ) {
			writer.Write ( value );
			return this;
		}
		public virtual int ReadInt32 ( ) {
			return reader.ReadInt32 ( );
		}
		public virtual ByteBuffer Write ( int off, int value ) {
			Position = off;
			writer.Write ( value );
			return this;
		}
		public virtual int ReadInt32 ( int off ) {
			Position = off;
			return reader.ReadInt32 ( );
		}

		/// <summary>
		/// Long
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( long value ) {
			writer.Write ( value );
			return this;
		}
		public virtual long ReadInt64 ( ) {
			return reader.ReadInt64 ( );
		}
		public virtual ByteBuffer Write ( int off, long value ) {
			Position = off;
			writer.Write ( value );
			return this;
		}
		public virtual long ReadInt64 ( int off ) {
			Position = off;
			return reader.ReadInt64 ( );
		}

		/// <summary>
		/// Float
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( float value ) {
			writer.Write ( value );
			return this;
		}
		public virtual float ReadSingle ( ) {
			return reader.ReadSingle ( );
		}
		public virtual ByteBuffer Write ( int off, float value ) {
			Position = off;
			writer.Write ( value );
			return this;
		}
		public virtual float ReadSingle ( int off ) {
			Position = off;
			return reader.ReadSingle ( );
		}

		/// <summary>
		/// Double
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( Double value ) {
			writer.Write ( value );
			return this;
		}
		public virtual Double ReadDouble ( ) {
			return reader.ReadDouble ( );
		}
		public virtual ByteBuffer Write ( int off, Double value ) {
			Position = off;
			writer.Write ( value );
			return this;
		}
		public virtual Double ReadDouble ( int off ) {
			Position = off;
			return reader.ReadDouble ( );
		}

		/// <summary>
		/// String
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( String value ) {
			writer.Write ( Encoding.ASCII.GetBytes ( value ) );
			return this;
		}
		public virtual String ReadString ( ) {
			Rewind ( );
			return ReadString ( length ( ) );
		}
		public virtual String ReadString ( int len ) {
			byte[] b = new byte[len];
			reader.Read ( b, 0, len );
			while ( len > 0 && b[len - 1] == 0 ) {
				--len;
			}
			return Encoding.ASCII.GetString ( b, 0, len ).Trim ( );
		}

		/// <summary>
		/// Boolean
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual ByteBuffer Write ( bool value ) {
			writer.Write ( value );
			return this;
		}
		public virtual bool ReadBoolean ( ) {
			return reader.ReadBoolean ( );
		}

		public virtual String ToString ( ) {
			System.Text.StringBuilder buf = new System.Text.StringBuilder ( );

			byte[] arr = ToArray ( );
			foreach ( byte b in arr ) {
				buf.Append ( String.Format ( "{0:X2} ", b ) );
			}

			return buf.ToString ( );
		}

		///////////////////////////////////////////////////////////////////////
		/// Public Class Methods
		///////////////////////////////////////////////////////////////////////

		public static ByteBuffer Wrap ( byte[] buf ) {
			return Wrap ( buf, ByteOrder.LITTLE_ENDIAN );
		}
		public static ByteBuffer Wrap ( byte[] buf, ByteOrder order ) {
			return new ByteBuffer ( buf, order );
		}

		public static ByteBuffer Wrap ( byte[] buf, int offset, int len ) {
			return Wrap ( buf, offset, len, ByteOrder.LITTLE_ENDIAN );
		}
		public static ByteBuffer Wrap ( byte[] buf, int offset, int len, ByteOrder order ) {
			byte[] newBuf = new byte[len];
			Array.Copy ( buf, offset, newBuf, 0, len );
			return new ByteBuffer ( newBuf, order );
		}
	}
}
