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

using System;
using System.IO;
using System.Net; 
namespace Managed.Adb.IO {
	/// <summary> 
	/// Big-Endian Binary Reader 
	/// </summary> 
	public class BEBinaryReader : BinaryReader {
		public BEBinaryReader ( MemoryStream s )
			: base ( s ) {
		}

		/// <summary> 
		/// Reads a 2-byte signed integer from the current stream and advances the  
		/// current position of the stream by two bytes. 
		/// </summary> 
		/// <returns></returns> 
		public override short ReadInt16 ( ) {
			return IPAddress.NetworkToHostOrder ( base.ReadInt16 ( ) );
		}

		/// <summary> 
		/// Reads a 4-byte signed integer from the current stream and advances the  
		/// current position of the stream by four bytes. 
		/// </summary> 
		/// <returns></returns> 
		public override int ReadInt32 ( ) {
			return IPAddress.NetworkToHostOrder ( base.ReadInt32 ( ) );
		}

		/// <summary> 
		/// Reads an 8-byte signed integer from the current stream and advances  
		/// the current position of the stream by four bytes. 
		/// </summary> 
		/// <returns></returns> 
		public override long ReadInt64 ( ) {
			return IPAddress.NetworkToHostOrder ( base.ReadInt64 ( ) );
		}

		/// <summary> 
		/// Reads a 4-byte floating point value from the current stream and  
		/// advances the current position of the stream by four bytes. 
		/// </summary> 
		/// <returns></returns> 
		public override float ReadSingle ( ) {
			byte[] temp = BitConverter.GetBytes ( base.ReadSingle ( ) );
			Array.Reverse ( temp );
			float returnVal = BitConverter.ToSingle ( temp, 0 );
			return returnVal;
		}

		/// <summary> 
		/// Reads an 8-byte floating point value from the current stream and  
		/// advances the current position of the stream by eight bytes. 
		/// </summary> 
		/// <returns></returns> 
		public override double ReadDouble ( ) {
			byte[] temp = BitConverter.GetBytes ( base.ReadDouble ( ) );
			Array.Reverse ( temp );
			double returnVal = BitConverter.ToDouble ( temp, 0 );
			return returnVal;
		}
	}
}
