using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Drawing;
using System.Threading;
using System.IO;
using Managed.Adb.Exceptions;
using Managed.Adb.Utilities.IO;
using Managed.Adb.Utilities.Conversion;

namespace Managed.Adb {
	internal class AdbHelper {
		private AdbHelper ( ) {

		}

		private static AdbHelper _instance = null;
		public static AdbHelper Instance {
			get {
				if ( _instance == null ) {
					_instance = new AdbHelper ( );
				}
				return _instance;
			}
		}

		private const int WAIT_TIME = 5;

		public Socket Open ( IPAddress address, int port ) {
			Socket s = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try {
				s.Connect ( address, port );
				s.Blocking = false;
				s.NoDelay = true;
				byte[] req = CreateAdbForwardRequest ( null, port );
				if ( !Write ( s, req ) ) {
					throw new IOException ( "failed submitting request to ADB" );
				}
				AdbResponse resp = ReadAdbResponse ( s, false );
				if ( !resp.Okay ) {
					throw new IOException ( "connection request rejected" );
				}
				s.Blocking = true;
			} catch ( IOException ) {
				s.Close ( );
				throw;
			}
			return s;
		}

		public byte[] CreateAdbForwardRequest ( String address, int port ) {
			String request;

			if ( address == null )
				request = "tcp:" + port;
			else
				request = "tcp:" + port + ":" + address;
			return FormAdbRequest ( request );
		}

		private byte[] FormAdbRequest ( String req ) {
			String resultStr = String.Format ( "{0}{1}", req.Length.ToString ( "X4" ), req ); //$NON-NLS-1$
			Console.WriteLine ( resultStr );
			byte[] result;
			try {
				result = Encoding.Default.GetBytes ( resultStr );
			} catch ( EncoderFallbackException efe ) {
				Console.WriteLine ( efe );
				return null;
			}

			System.Diagnostics.Debug.Assert ( result.Length == req.Length + 4, String.Format ( "result: {0}\nreq: {0}", result.Length, req.Length ) );
			return result;
		}

		private bool Write ( Socket socket, byte[] data ) {
			try {
				Write ( socket, data, -1, 5 * 1000 );
			} catch ( IOException e ) {
				Console.WriteLine ( e );
				return false;
			}

			return true;
		}

		private void Write ( Socket socket, byte[] data, int length, int timeout ) {
			using ( var buf = new MemoryStream ( data, 0, length != -1 ? length : data.Length ) ) {
				int numWaits = 0;

				//while ( buf.Position != buf.Length ) {
				try {
					int count;
					Console.WriteLine ( Encoding.Default.GetString ( data ) );
					count = socket.Send ( buf.ToArray ( ) );
					if ( count < 0 ) {
						throw new IOException ( "channel EOF" );
					} else if ( count == 0 ) {
						// TODO: need more accurate timeout?
						if ( timeout != 0 && numWaits * WAIT_TIME > timeout ) {
							throw new IOException ( "timeout" );
						}
						// non-blocking spin
						Thread.Sleep ( WAIT_TIME );
						numWaits++;
					} else {
						numWaits = 0;
					}
				} catch ( SocketException sex ) {
					Console.WriteLine ( sex );
					throw;
				}
				//}
			}
		}

		AdbResponse ReadAdbResponse ( Socket socket, bool readDiagString ) {

			AdbResponse resp = new AdbResponse ( );

			byte[] reply = new byte[4];
			if ( Read ( socket, reply ) == false ) {
				return resp;
			}
			resp.IOSuccess = true;

			if ( IsOkay ( reply ) ) {
				resp.Okay = true;
			} else {
				readDiagString = true; // look for a reason after the FAIL
				resp.Okay = false;
			}

			// not a loop -- use "while" so we can use "break"
			while ( readDiagString ) {
				// length string is in next 4 bytes
				byte[] lenBuf = new byte[4];
				if ( Read ( socket, lenBuf ) == false ) {
					Console.WriteLine ( "Expected diagnostic string not found" );
					break;
				}

				String lenStr = ReplyToString ( lenBuf );

				int len;
				try {
					len = int.Parse ( lenStr, System.Globalization.NumberStyles.HexNumber );

				} catch ( FormatException nfe ) {
					Console.WriteLine ( "Expected digits, got '" + lenStr + "': "
										+ lenBuf[0] + " " + lenBuf[1] + " " + lenBuf[2] + " "
										+ lenBuf[3] );
					Console.WriteLine ( "reply was " + ReplyToString ( reply ) );
					break;
				}

				byte[] msg = new byte[len];
				if ( Read ( socket, msg ) == false ) {
					Console.WriteLine ( "Failed reading diagnostic string, len=" + len );
					break;
				}

				resp.Message = ReplyToString ( msg );
				Console.WriteLine ( "Got reply '" + ReplyToString ( reply ) + "', diag='"
								+ resp.Message + "'" );

				break;
			}

			return resp;
		}

		private bool Read ( Socket socket, byte[] data ) {
			try {
				Read ( socket, data, -1, 5 * 1000 );
			} catch ( IOException e ) {
				Console.WriteLine ( "readAll: IOException: " + e.Message );
				return false;
			}

			return true;
		}

		private void Read ( Socket socket, byte[] data, int length, int timeout ) {
			using ( var buf = new MemoryStream ( data, 0, length != -1 ? length : data.Length ) ) {
				int numWaits = 0;

				while ( buf.Position != buf.Length ) {
					int count;
					try {
						count = socket.Receive ( data );
						if ( count < 0 ) {
							Console.WriteLine ( "read: channel EOF" );
							throw new IOException ( "EOF" );
						} else if ( count == 0 ) {
							// TODO: need more accurate timeout?
							if ( timeout != 0 && numWaits * WAIT_TIME > timeout ) {
								Console.WriteLine ( "read: timeout" );
								throw new IOException ( "timeout" );
							}
							// non-blocking spin
							Thread.Sleep ( WAIT_TIME );
							numWaits++;
						} else {
							numWaits = 0;
						}
					} catch ( SocketException sex ) {
						throw new IOException ( "No Data to read" );
					}
				}
			}
		}

		private byte[] CreateJdwpForwardRequest ( int pid ) {
			String req = String.Format ( "jdwp:{0}", pid );
			return FormAdbRequest ( req );
		}

		public bool CreateForward ( IPEndPoint adbSockAddr, Device device, int localPort, int remotePort ) {

			Socket adbChan = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try {
				adbChan.Connect ( adbSockAddr );
				adbChan.Blocking = false;

				byte[] request = FormAdbRequest ( String.Format ( "host-serial:{0}:forward:tcp:{1};tcp:{2}", //$NON-NLS-1$
								device.getSerialNumber ( ), localPort, remotePort ) );

				if ( Write ( adbChan, request ) == false ) {
					throw new IOException ( "failed to submit the forward command." );
				}

				AdbResponse resp = ReadAdbResponse ( adbChan, false /* readDiagString */);
				if ( !resp.IOSuccess || !resp.Okay ) {
					throw new IOException ( "Device rejected command: " + resp.Message );
				}
			} finally {
				if ( adbChan != null ) {
					adbChan.Close ( );
				}
			}

			return true;
		}

		private bool IsOkay ( byte[] reply ) {
			return reply[0] == (byte)'O' && reply[1] == (byte)'K'
								&& reply[2] == (byte)'A' && reply[3] == (byte)'Y';
		}

		private String ReplyToString ( byte[] reply ) {
			String result;
			try {
				result = Encoding.Default.GetString ( reply );
			} catch ( DecoderFallbackException uee ) {
				Console.WriteLine ( uee );
				result = "";
			}
			return result;
		}

		public RawImage GetFrameBuffer ( IPEndPoint adbSockAddr ) {

			RawImage imageParams = new RawImage ( );
			byte[] request = FormAdbRequest ( "framebuffer:" ); //$NON-NLS-1$
			byte[] nudge = {
            0
        };
			byte[] reply;

			Socket adbChan = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try {
				adbChan.Connect ( adbSockAddr );
				adbChan.Blocking = false;
				adbChan.NoDelay = true;

				// if the device is not -1, then we first tell adb we're looking to talk
				// to a specific device
				//setDevice(adbChan, device);

				if ( Write ( adbChan, request ) == false )
					throw new IOException ( "failed asking for frame buffer" );

				AdbResponse resp = ReadAdbResponse ( adbChan, false /* readDiagString */);
				if ( !resp.IOSuccess || !resp.Okay ) {
					Console.WriteLine ( "Got timeout or unhappy response from ADB fb req: "
									+ resp.Message );
					adbChan.Close ( );
					return null;
				}

				// first the protocol version.
				reply = new byte[4];
				if ( Read ( adbChan, reply ) == false ) {
					Console.WriteLine ( "got partial reply from ADB fb:" );

					adbChan.Close ( );
					return null;
				}
				EndianBinaryReader buf;
				int version = 0;
				using ( MemoryStream ms = new MemoryStream ( reply ) ) {
					buf = new EndianBinaryReader ( EndianBitConverter.Little, ms );

					version = buf.ReadInt32 ( );
				}

				// get the header size (this is a count of int)
				int headerSize = RawImage.GetHeaderSize ( version );
				// read the header
				reply = new byte[headerSize * 4];
				if ( Read ( adbChan, reply ) == false ) {
					Console.WriteLine ( "got partial reply from ADB fb:" );

					adbChan.Close ( );
					return null;
				}
				using ( MemoryStream ms = new MemoryStream ( reply ) ) {
					buf = new EndianBinaryReader ( EndianBitConverter.Little, ms );

					// fill the RawImage with the header
					if ( imageParams.ReadHeader ( version, buf ) == false ) {
						Console.WriteLine ( "Screenshot", "Unsupported protocol: " + version );
						return null;
					}
				}

				Console.WriteLine ( "ddms", "image params: bpp=" + imageParams.Bpp + ", size="
								+ imageParams.Size + ", width=" + imageParams.Width
								+ ", height=" + imageParams.Height );

				if ( Write ( adbChan, nudge ) == false )
					throw new IOException ( "failed nudging" );

				reply = new byte[imageParams.Size];
				if ( Read ( adbChan, reply ) == false ) {
					Console.WriteLine ( "got truncated reply from ADB fb data" );
					adbChan.Close ( );
					return null;
				}

				imageParams.Data = reply;
			} finally {
				if ( adbChan != null ) {
					adbChan.Close ( );
				}
			}

			return imageParams;
		}

		private void setDevice ( Socket adbChan, Device device ) {
			// if the device is not -1, then we first tell adb we're looking to talk
			// to a specific device
			if ( device != null ) {
				String msg = "host:transport:" + device.GetSerialNumber ( ); //$NON-NLS-1$
				byte[] device_query = FormAdbRequest ( msg );

				if ( Write ( adbChan, device_query ) == false ) {
					throw new IOException ( "failed submitting device (" + device + ") request to ADB" );
				}

				AdbResponse resp = ReadAdbResponse ( adbChan, false /* readDiagString */);
				if ( !resp.Okay ) {
					throw new IOException ( "device (" + device + ") request rejected: " + resp.Message );
				}
			}

		}

		public void Reboot ( String into, IPEndPoint adbSockAddr, Device device ) {
			byte[] request;
			if ( into == null ) {
				request = FormAdbRequest ( "reboot:" ); //$NON-NLS-1$
			} else {
				request = FormAdbRequest ( "reboot:" + into ); //$NON-NLS-1$
			}

			Socket adbChan = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try {
				adbChan.Connect ( adbSockAddr );
				adbChan.Blocking = false;

				// if the device is not -1, then we first tell adb we're looking to talk
				// to a specific device
				setDevice ( adbChan, device );

				if ( Write ( adbChan, request ) == false ) {
					throw new IOException ( "failed asking for reboot" );
				}
			} finally {
				if ( adbChan != null ) {
					adbChan.Close ( );
				}
			}
		}

	}
}
