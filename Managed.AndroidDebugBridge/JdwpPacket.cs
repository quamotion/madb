using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Managed.Adb.Utilities.IO;
using Managed.Adb.Utilities.Conversion;
using System.Net.Sockets;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public sealed class JdwpPacket {
		public const int JDWP_HEADER_LEN = 11;

		// results from findHandshake
		public const int HANDSHAKE_GOOD = 1;
		public const int HANDSHAKE_NOTYET = 2;
		public const int HANDSHAKE_BAD = 3;

		// our cmdSet/cmd
		private const int DDMS_CMD_SET = 0xc7;       // 'G' + 128
		private const int DDMS_CMD = 0x01;

		// "flags" field
		private const int REPLY_PACKET = 0x80;

		// this is sent and expected at the start of a JDWP connection
		private const String HANDSHAKE = "JDWP-Handshake";

		public const int HANDSHAKE_LEN = HANDSHAKE.Length;

		public JdwpPacket ( EndianBinaryWriter buf ) {
			Buffer = buf;
			IsNew = true;
			SerialID = 0x40000000;
		}

		private int SerialID { get; set; }

		private EndianBinaryWriter Buffer { get; private set; }
		public int Length { get; private set; }
		public int ID { get; private set; }
		public int Flags { get; private set; }
		public int CommandSet { get; private set; }
		public int Command { get; private set; }
		public int ErrorCode { get; private set; }
		private bool IsNew { get; set; }

		/// <summary>
		/// Finish a packet created with newPacket().
		/// This always creates a command packet, with the next serial number
		/// in sequence.
		/// We have to take "payloadLength" as an argument because we can't
		/// see the position in the "slice" returned by getPayload().  We could
		/// fish it out of the chunk header, but it's legal for there to be
		/// more than one chunk in a JDWP packet.
		/// On exit, "position" points to the end of the data.		
		/// </summary>
		/// <param name="payloadLength">Length of the payload.</param>
		void finishPacket ( int payloadLength ) {
			System.Diagnostics.Debug.Assert ( IsNew );

			EndianBitConverter oldOrder = Buffer.BitConverter;
			if ( ChunkHandler.CHUNK_ORDER == ChunkHandler.ByteOrder.LittleEndian ) {
				if ( Buffer.BitConverter != EndianBitConverter.Little ) {
					Buffer = new EndianBinaryWriter ( EndianBitConverter.Little, Buffer.BaseStream );
				}
			}

			Length = JDWP_HEADER_LEN + payloadLength;
			ID = GetNextSerial ( );
			Flags = 0;
			CommandSet = DDMS_CMD_SET;
			Command = DDMS_CMD;

			Buffer.Write ( Length );
			Buffer.Write ( ID );
			Buffer.Write ( Flags );
			Buffer.Write ( CommandSet );
			Buffer.Write ( Command );

			if ( oldOrder == EndianBitConverter.Little ) {
				if ( Buffer.BitConverter != EndianBitConverter.Little ) {
					Buffer = new EndianBinaryWriter ( EndianBitConverter.Little, Buffer.BaseStream );
				}
			} else {
				if ( Buffer.BitConverter != EndianBitConverter.Big ) {
					Buffer = new EndianBinaryWriter ( EndianBitConverter.Big, Buffer.BaseStream );
				}
			}

			// move to end
			Buffer.BaseStream.Position = Length;
		}


		/// <summary>
		/// Get the next serial number.  This creates a unique serial number
		/// across all connections, not just for the current connection.  This
		/// is a useful property when debugging, but isn't necessary.
		/// We can't synchronize on an int, so we use a sync method.
		/// </summary>
		/// <returns></returns>
		private int GetNextSerial ( ) {
			lock ( this ) {
				return SerialID++;
			}
		}

		public EndianBinaryReader GetPayload ( ) {
			MemoryStream buf;
			long oldPosn = Buffer.BaseStream.Position;

			Buffer.BaseStream.Position = JDWP_HEADER_LEN;
			byte[] data = ( Buffer.BaseStream as MemoryStream ).ToArray ( );
			byte[] sdata = new byte[data.Length - JDWP_HEADER_LEN];
			data.CopyTo ( sdata, JDWP_HEADER_LEN );


			Buffer.BaseStream.Position = oldPosn;

			if ( Length > 0 ) {
				buf = new MemoryStream ( Length - JDWP_HEADER_LEN );
				buf.Write ( sdata, 0, Length - JDWP_HEADER_LEN );
			} else {
				System.Diagnostics.Debug.Assert ( IsNew );
			}

			EndianBinaryReader ebr;
			if ( ChunkHandler.CHUNK_ORDER == ChunkHandler.ByteOrder.LittleEndian ) {
				ebr = new EndianBinaryReader ( EndianBitConverter.Little, buf );
			} else {
				ebr = new EndianBinaryReader ( EndianBitConverter.Big, buf );
			}

			return ebr;
		}

		/**
     * Returns "true" if this JDWP packet has a JDWP command type.
     *
     * This never returns "true" for reply packets.
     */
		public bool IsDdmPacket {
			get {
				return ( Flags & REPLY_PACKET ) == 0 &&
							 CommandSet == DDMS_CMD_SET &&
							 Command == DDMS_CMD;
			}
		}

		/**
		 * Returns "true" if this JDWP packet is tagged as a reply.
		 */
		public bool IsReply {
			get {
				return ( Flags & REPLY_PACKET ) != 0;
			}
		}

		/**
     * Returns "true" if this JDWP packet is a reply with a nonzero
     * error code.
     */
		public bool IsError {
			get {
				return IsReply && ErrorCode != 0;
			}
		}

		/**
		 * Returns "true" if this JDWP packet has no data.
		 */
		public bool IsEmpty {
			get {
				return ( Length == JDWP_HEADER_LEN );
			}
		}

		void WriteAndConsume ( Socket chan ) {
			System.Diagnostics.Debug.Assert ( Length > 0 );

			MemoryStream tms = new MemoryStream ( (int)Buffer.BaseStream.Position );
			byte[] tdata = new byte[Buffer.BaseStream.Position];
			int oldLimit = Length;
			byte[] fdata = ( Buffer.BaseStream as MemoryStream ).ToArray ( );
			for ( int i = 0; i < tdata.Length; i++ ) {
				tdata[i] = fdata[i];
			}

			chan.Send ( tdata );

		}

		/**
		 * "Move" the packet data out of the buffer we're sitting on and into
		 * buf at the current position.
		 */
		void MovePacket ( EndianBinaryWriter buf ) {
			Log.v ( "ddms", "moving " + Length + " bytes" );
			int oldPosn = (int)Buffer.BaseStream.Position;
			Buffer.BaseStream.Position = 0;


			byte[] tdata = new byte[oldPosn];
			byte[] fdata = ( Buffer.BaseStream as MemoryStream ).ToArray ( );
			for ( int i = 0; i < tdata.Length; i++ ) {
				tdata[i] = fdata[i];
			}

			buf.Write ( tdata );
			Buffer.BaseStream.Position = Length;
		}

		void consume ( ) {
			//Log.d("ddms", "consuming " + mLength + " bytes");
			//Log.d("ddms", "  posn=" + mBuffer.position()
			//    + ", limit=" + mBuffer.limit());

			/*
			 * The "flip" call sets "limit" equal to the position (usually the
			 * end of data) and "position" equal to zero.
			 *
			 * compact() copies everything from "position" and "limit" to the
			 * start of the buffer, sets "position" to the end of data, and
			 * sets "limit" to the capacity.
			 *
			 * On entry, "position" is set to the amount of data in the buffer
			 * and "limit" is set to the capacity.  We want to call flip()
			 * so that position..limit spans our data, advance "position" past
			 * the current packet, then compact.
			 */

			byte[] tdata = new byte[Buffer.BaseStream.Position];
			byte[] fdata = ( Buffer.BaseStream as MemoryStream ).ToArray ( );
			for ( int i = 0; i < tdata.Length; i++ ) {
				tdata[i] = fdata[i];
			}

			mBuffer.flip ( );         // limit<-posn, posn<-0
			mBuffer.position ( mLength );
			mBuffer.compact ( );      // shift posn...limit, posn<-pending data
			mLength = 0;
			//Log.d("ddms", "  after compact, posn=" + mBuffer.position()
			//    + ", limit=" + mBuffer.limit());
		}
	}
}
