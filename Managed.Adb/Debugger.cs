using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace Managed.Adb {
	public class Debugger : IPacketConsumer {
		public enum ConnectionStates {
			NotConnected = 1,
			AwaitShake = 2,
			Ready = 3
		}
		private const int INITIAL_BUF_SIZE = 1 * 1024;
		private const int MAX_BUF_SIZE = 32 * 1024;

		private const int PRE_DATA_BUF_SIZE = 256;

		public Debugger ( IClient client, int listenPort ) {
			this.Client = client;
			this.ListenPort = listenPort;

			IPEndPoint endPoint = new IPEndPoint ( IPAddress.Loopback, listenPort );
			ListenChannel = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			ListenChannel.Blocking = false;
			ListenChannel.ExclusiveAddressUse = false;
			ListenChannel.Bind ( endPoint );

			ConnectionState = ConnectionStates.NotConnected;
			Log.d ( "ddms", this.ToString ( ) );
		}


		public BinaryReader ReadBuffer { get; set; }
		public BinaryWriter PreDataBuffer { get; set; }
		public Socket ListenChannel { get; set; }
		public Socket Channel { get; set; }
		public IClient Client { get; set; }
		public int ListenPort { get; set; }
		public ConnectionStates ConnectionState { get; set; }

		public bool IsDebuggerAttached { get; private set; }


		public void Read ( ) {
			throw new NotImplementedException ( );
			int count;

			/*if ( mReadBuffer.position ( ) == mReadBuffer.capacity ( ) ) {
				if ( mReadBuffer.capacity ( ) * 2 > MAX_BUF_SIZE ) {
					throw new BufferOverflowException ( );
				}
				Log.d ( "ddms", "Expanding read buffer to "
						+ mReadBuffer.capacity ( ) * 2 );

				ByteBuffer newBuffer =
								ByteBuffer.allocate ( mReadBuffer.capacity ( ) * 2 );
				mReadBuffer.position ( 0 );
				newBuffer.put ( mReadBuffer );     // leaves "position" at end

				mReadBuffer = newBuffer;
			}

			count = mChannel.read ( mReadBuffer );
			Log.v ( "ddms", "Read " + count + " bytes from " + this );
			if ( count < 0 ) throw new IOException ( "read failed" );*/
		}

		public /*JdwpPacket*/ Object GetJdwpPacket ( ) {
			throw new NotImplementedException ( );

			/*if (mConnState == ST_AWAIT_SHAKE) {
            int result;

            result = JdwpPacket.findHandshake(mReadBuffer);
            //Log.v("ddms", "findHand: " + result);
            switch (result) {
                case JdwpPacket.HANDSHAKE_GOOD:
                    Log.d("ddms", "Good handshake from debugger");
                    JdwpPacket.consumeHandshake(mReadBuffer);
                    sendHandshake();
                    mConnState = ST_READY;

                    ClientData cd = mClient.getClientData();
                    cd.setDebuggerConnectionStatus(DebuggerStatus.ATTACHED);
                    mClient.update(Client.CHANGE_DEBUGGER_STATUS);

                    // see if we have another packet in the buffer
                    return getJdwpPacket();
                case JdwpPacket.HANDSHAKE_BAD:
                    // not a debugger, throw an exception so we drop the line
                    Log.d("ddms", "Bad handshake from debugger");
                    throw new IOException("bad handshake");
                case JdwpPacket.HANDSHAKE_NOTYET:
                    break;
                default:
                    Log.e("ddms", "Unknown packet while waiting for client handshake");
            }
            return null;
        } else if (mConnState == ST_READY) {
            if (mReadBuffer.position() != 0) {
                Log.v("ddms", "Checking " + mReadBuffer.position() + " bytes");
            }
            return JdwpPacket.findPacket(mReadBuffer);
        } else {
            Log.e("ddms", "Receiving data in state = " + mConnState);
        }

        return null;*/
		}

		// TODO: JdwpPacket
		public void ForwardPacketToClient ( /*JdwpPacket*/ Object packet ) {
			Client.SendAndConsume ( packet );
		}

		public bool SendHandshake ( ) {
			throw new NotImplementedException ( );
			/*ByteBuffer tempBuffer = ByteBuffer.allocate ( JdwpPacket.HANDSHAKE_LEN );
			JdwpPacket.PutHandshake ( tempBuffer );
			int expectedLength = tempBuffer.position ( );
			tempBuffer.flip ( );
			if ( Channel.Send ( tempBuffer ) != expectedLength ) {
				throw new IOException ( "partial handshake write" );
			}

			expectedLength = PreDataBuffer.BaseStream.Position;
			if ( expectedLength > 0 ) {
				Log.d ( "ddms", "Sending " + PreDataBuffer.BaseStream.Position
								+ " bytes of saved data" );
				//PreDataBuffer.flip ( );
				if ( Channel.Send ( PreDataBuffer ) != expectedLength ) {
					throw new IOException ( "partial pre-data write" );
				}
				PreDataBuffer.
			}*/
		}

		//TODO: JdwpPacket
		public void SendAndConsume ( /*JdwpPacket*/ Object packet ) {
			if ( Channel == null ) {
				/*
				 * Buffer this up so we can send it to the debugger when it
				 * finally does connect.  This is essential because the VM_START
				 * message might be telling the debugger that the VM is
				 * suspended.  The alternative approach would be for us to
				 * capture and interpret VM_START and send it later if we
				 * didn't choose to un-suspend the VM for our own purposes.
				 */
				//Log.d ( "ddms", "Saving packet 0x" + packet.ID.ToString ( "X" ) );
				//packet.MovePacket ( PreDataBuffer );
			} else {
				//packet.WriteAndConsume ( Channel );
			}
		}

		//		public voidr RegisterListener

		public Socket Accept ( ) {
			return Accept ( ListenChannel );
		}

		public Socket Accept ( Socket listenChan ) {
			lock ( listenChan ) {
				if ( listenChan != null ) {
					Socket newChan = listenChan.Accept ( );
					if ( Channel != null ) {
						Log.w ( "ddms", "debugger already talking to " + Client.ToString ( ) + " on " + ListenPort.ToString ( ) );
						newChan.Close ( );
						return null;
					}

					Channel = newChan;
					Channel.Blocking = false;
					ConnectionState = ConnectionStates.AwaitShake;
					return Channel;
				}

				return null;
			}
		}

		public void CloseData ( ) {
			try {
				if ( Channel != null ) {
					Channel.Close ( );
					Channel = null;
					ConnectionState = ConnectionStates.NotConnected;

					//TODO: ClientData
					/*ClientData cd = Client.ClientData;
					cd.DebuggerConnectionStatus = DebuggerStatus.Default;*/
					Client.Update ( ClientChangeMask.ChangeDebuggerStatus );

				}
			} catch ( IOException ioe ) {
				Log.w ( "ddms", ioe );
			}
		}

		public void Close ( ) {
			try {
				if ( ListenChannel != null ) {
					ListenChannel.Close ( );
				}
				ListenChannel = null;
				CloseData ( );
			} catch ( IOException ioe ) {
				Log.w ( "ddms", ioe );
			}
		}

		public override string ToString ( ) {
			// mChannel != null means we have connection, ST_READY means it's going
			return "[Debugger " + ListenPort + "-->" + Client.ClientData/*.Pid*/
							+ ( ( ConnectionState != ConnectionStates.Ready ) ? " inactive]" : " active]" );
		}
	}
}
