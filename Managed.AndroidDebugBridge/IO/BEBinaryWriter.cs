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
	using System.Net;

	/// <summary> 
	/// Big-Endian Binary Writer 
	/// </summary> 
	/// <ignore>true</ignore>
	public class BEBinaryWriter : BinaryWriter {
		public BEBinaryWriter ( MemoryStream s )
			: base ( s ) {
		}

		/// <summary> 
		/// Writes a two-byte signed integer to the current stream and advances the stream  
		/// position by two bytes. 
		/// </summary> 
		/// <param name="value"></param> 
		public override void Write ( short value ) {
			base.Write ( IPAddress.HostToNetworkOrder ( value ) );
		}

		/// <summary> 
		/// Writes a four-byte signed integer to the current stream and advances the stream  
		/// position by four bytes. 
		/// </summary> 
		/// <param name="value"></param> 
		public override void Write ( int value ) {
			base.Write ( IPAddress.HostToNetworkOrder ( value ) );
		}

		/// <summary> 
		/// Writes an eight-byte signed integer to the current stream and advances the stream  
		/// position by eight bytes. 
		/// </summary> 
		/// <param name="value"></param> 
		public override void Write ( long value ) {
			base.Write ( IPAddress.HostToNetworkOrder ( value ) );
		}

		/// <summary> 
		/// Writes a four-byte floating-point value to the current stream and advances the  
		/// stream position by four bytes. 
		/// </summary> 
		/// <param name="value"></param> 
		public override void Write ( float value ) {
			byte[] temp = BitConverter.GetBytes ( value );
			Array.Reverse ( temp );
			base.Write ( BitConverter.ToSingle ( temp, 0 ) );
		}

		/// <summary> 
		/// Writes an eight-byte floating-point value to the current stream and advances  
		/// the stream position by eight bytes. 
		/// </summary> 
		/// <param name="value"></param> 
		public override void Write ( double value ) {
			byte[] temp = BitConverter.GetBytes ( value );
			Array.Reverse ( temp );
			base.Write ( BitConverter.ToDouble ( temp, 0 ) );
		}
	}
}
