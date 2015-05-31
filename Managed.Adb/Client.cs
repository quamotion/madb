using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Managed.Adb {
	/// <summary>
	/// A debug client
	/// </summary>
	public class Client : IClient {


		/// <summary>
		/// 
		/// </summary>
		private const int SERVER_PROTOCOL_VERSION = 1;
		/// <summary>
		/// 
		/// </summary>
		private const int INITIAL_BUF_SIZE = 2 * 1024;
		/// <summary>
		/// 
		/// </summary>
		private const int MAX_BUF_SIZE = 200 * 1024 * 1024;
		/// <summary>
		/// 
		/// </summary>
		private const int WRITE_BUF_SIZE = 256;

		/// <summary>
		/// Initializes a new instance of the <see cref="Client"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="channel">The channel.</param>
		/// <param name="pid">The pid.</param>
		public Client ( Device device, Socket channel, int pid ) {
			this.Device = device;
			this.Channel = channel;
			this.ClientData = new ClientData ( pid );

			IsThreadUpdateEnabled = DdmPreferences.InitialThreadUpdate;
			IsHeapUpdateEnabled = DdmPreferences.InitialHeapUpdate;
			ConnectionState = ClientConnectionState.Init;
		}

		/// <summary>
		/// Gets the state of the change.
		/// </summary>
		/// <value>
		/// The state of the change.
		/// </value>
		public ClientChangeState ChangeState { get; private set; }

		/// <summary>
		/// Gets or sets the channel.
		/// </summary>
		/// <value>
		/// The channel.
		/// </value>
		public Socket Channel { get; set; }

		/// <summary>
		/// Gets the state of the connection.
		/// </summary>
		/// <value>
		/// The state of the connection.
		/// </value>
		public ClientConnectionState ConnectionState { get; private set; }

		/// <summary>
		/// Gets the device.
		/// </summary>
		public IDevice Device { get; private set; }

		/// <summary>
		/// Gets the device implementation.
		/// </summary>
		public Device DeviceImplementation { get; private set; }

		/// <summary>
		/// Gets the debugger listen port.
		/// </summary>
		public int DebuggerListenPort { get; private set; }

		/// <summary>
		/// Returns <code>true</code> if the client VM is DDM-aware.
		/// </summary>
		/// <remarks>Calling here is only allowed after the connection has been established.</remarks>
		public bool IsDdmAware {
			get {
				switch ( ConnectionState ) {
					case ClientConnectionState.Init:
					case ClientConnectionState.NotJDWP:
					case ClientConnectionState.AwaitShake:
					case ClientConnectionState.NeedDDMPacket:
					case ClientConnectionState.NotDDM:
					case ClientConnectionState.Error:
					case ClientConnectionState.Disconnected:
						return false;
					case ClientConnectionState.Ready:
						return true;
					default:
						Log.e ( "ddm", "wtf are we doing in here? You shouldn't see this." );
						return false;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is debugger attached.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is debugger attached; otherwise, <c>false</c>.
		/// </value>
		public bool IsDebuggerAttached {
			get {
				return Debugger != null && Debugger.IsDebuggerAttached;
			}
		}

		/// <summary>
		/// Gets the debugger.
		/// </summary>
		public Debugger Debugger { get; private set; }

		/// <summary>
		/// Gets the client data.
		/// </summary>
		public ClientData ClientData { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is thread update enabled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is thread update enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsThreadUpdateEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is heap update enabled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is heap update enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsHeapUpdateEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is selected client.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is selected client; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelectedClient { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance is valid.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
		/// </value>
		public bool IsValid { get; private set; }

		/// <summary>
		/// Executes the garbage collector.
		/// </summary>
		public void ExecuteGarbageCollector ( ) {
			throw new NotImplementedException ( );

			/*try {
				HandleHeap.SendHPGC ( this );
			} catch ( IOException ioe ) {
				Log.w ( "ddms", "Send of HPGC message failed" );
			}*/
		}

		/// <summary>
		/// Dumps the hprof.
		/// </summary>
		public void DumpHprof ( ) {
			throw new NotImplementedException ( );

			/*bool canStream = ClientData.HasFeature ( ClientData.FEATURE_HPROF_STREAMING );
			try {
				if ( canStream ) {
					HandleHeap.SendHPDS ( this );
				} else {
					String file = String.Format("/sdcard/{0}.hprof", ClientData.ClientDescription.ReplaceAll ( "\\:.*", "" ) );
					HandleHeap.SendHPDU ( this, file );
				}
			} catch ( IOException e ) {
				Log.w ( "ddms", "Send of HPDU message failed" );
			}*/
		}

		/// <summary>
		/// Toggles the method profiling.
		/// </summary>
		public void ToggleMethodProfiling ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Requests the method profiling status.
		/// </summary>
		public void RequestMethodProfilingStatus ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Requests the thread update.
		/// </summary>
		public void RequestThreadUpdate ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Requests the thread stack trace.
		/// </summary>
		/// <param name="threadID">The thread ID.</param>
		public void RequestThreadStackTrace ( int threadID ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Requests the native heap information.
		/// </summary>
		/// <returns></returns>
		public bool RequestNativeHeapInformation ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Enables the allocation tracker.
		/// </summary>
		/// <param name="enable">if set to <c>true</c> [enable].</param>
		public void EnableAllocationTracker ( bool enable ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Requests the allocation status.
		/// </summary>
		public void RequestAllocationStatus ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Requests the allocation details.
		/// </summary>
		public void RequestAllocationDetails ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Kills this instance.
		/// </summary>
		public void Kill ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Registers the specified selector.
		/// </summary>
		/// <param name="selector">The selector.</param>
		public void Register ( object selector ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Listens for debugger.
		/// </summary>
		/// <param name="listenPort">The listen port.</param>
		public void ListenForDebugger ( int listenPort ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Sends the and consume.
		/// </summary>
		/// <param name="packet">The packet.</param>
		/// <param name="replyHandler">The reply handler.</param>
		public void SendAndConsume ( object packet, ChunkHandler replyHandler ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Adds the request id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="handler">The handler.</param>
		public void AddRequestId ( int id, ChunkHandler handler ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Removes the request id.
		/// </summary>
		/// <param name="id">The id.</param>
		public void RemoveRequestId ( int id ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Determines whether [is response to us] [the specified id].
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public ChunkHandler IsResponseToUs ( int id ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Packets the failed.
		/// </summary>
		/// <param name="packet">The packet.</param>
		public void PacketFailed ( object packet ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// DDMs the seen.
		/// </summary>
		/// <returns></returns>
		public bool DdmSeen ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Closes the specified notify.
		/// </summary>
		/// <param name="notify">if set to <c>true</c> [notify].</param>
		public void Close ( bool notify ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Updates the specified change mask.
		/// </summary>
		/// <param name="changeMask">The change mask.</param>
		public void Update ( ClientChangeMask changeMask ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Reads this instance.
		/// </summary>
		public void Read ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Gets the JDWP packet.
		/// </summary>
		/// <returns></returns>
		public object GetJdwpPacket ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Forwards the packet to client.
		/// </summary>
		/// <param name="packet">The packet.</param>
		public void ForwardPacketToClient ( object packet ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Sends the handshake.
		/// </summary>
		/// <returns></returns>
		public bool SendHandshake ( ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Sends the and consume.
		/// </summary>
		/// <param name="packet">The packet.</param>
		public void SendAndConsume ( object packet ) {
			throw new NotImplementedException ( );
		}


		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override String ToString ( ) {
			return String.Format ( "[Client pid: {0}]", ClientData.Pid );
		}
	}
}
