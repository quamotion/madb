using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Managed.Adb {
	public class Client : IClient {


		private const int SERVER_PROTOCOL_VERSION = 1;
		private const int INITIAL_BUF_SIZE = 2 * 1024;
		private const int MAX_BUF_SIZE = 200 * 1024 * 1024;
		private const int WRITE_BUF_SIZE = 256;

		public Client ( Device device, Socket channel, int pid ) {
			this.Device = device;
			this.Channel = channel;
			this.ClientData = new ClientData ( pid );

			IsThreadUpdateEnabled = DdmPreferences.InitialThreadUpdate;
			IsHeapUpdateEnabled = DdmPreferences.InitialHeapUpdate;
			ConnectionState = ClientConnectionState.Init;
		}

		public ClientChangeState ChangeState { get; private set; }

		public Socket Channel { get; set; }

		public ClientConnectionState ConnectionState { get; private set; }

		public IDevice Device { get; private set; }

		public Device DeviceImplementation { get; private set; }

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

		public bool IsDebuggerAttached {
			get {
				return Debugger != null && Debugger.IsDebuggerAttached;
			}
		}

		public Debugger Debugger { get; private set; }

		public ClientData ClientData { get; private set; }

		public bool IsThreadUpdateEnabled { get; set; }

		public bool IsHeapUpdateEnabled { get; set; }

		public bool IsSelectedClient { get; set; }

		public bool IsValid { get; private set; }

		public void ExecuteGarbageCollector ( ) {
			throw new NotImplementedException ( );

			/*try {
				HandleHeap.SendHPGC ( this );
			} catch ( IOException ioe ) {
				Log.w ( "ddms", "Send of HPGC message failed" );
			}*/
		}

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

		public void ToggleMethodProfiling ( ) {
			throw new NotImplementedException ( );
		}

		public void RequestMethodProfilingStatus ( ) {
			throw new NotImplementedException ( );
		}

		public void RequestThreadUpdate ( ) {
			throw new NotImplementedException ( );
		}

		public void RequestThreadStackTrace ( int threadID ) {
			throw new NotImplementedException ( );
		}

		public bool RequestNativeHeapInformation ( ) {
			throw new NotImplementedException ( );
		}

		public void EnableAllocationTracker ( bool enable ) {
			throw new NotImplementedException ( );
		}

		public void RequestAllocationStatus ( ) {
			throw new NotImplementedException ( );
		}

		public void RequestAllocationDetails ( ) {
			throw new NotImplementedException ( );
		}

		public void Kill ( ) {
			throw new NotImplementedException ( );
		}

		public void Register ( object selector ) {
			throw new NotImplementedException ( );
		}

		public void ListenForDebugger ( int listenPort ) {
			throw new NotImplementedException ( );
		}

		public void SendAndConsume ( object packet, ChunkHandler replyHandler ) {
			throw new NotImplementedException ( );
		}

		public void AddRequestId ( int id, ChunkHandler handler ) {
			throw new NotImplementedException ( );
		}

		public void RemoveRequestId ( int id ) {
			throw new NotImplementedException ( );
		}

		public ChunkHandler IsResponseToUs ( int id ) {
			throw new NotImplementedException ( );
		}

		public void PacketFailed ( object packet ) {
			throw new NotImplementedException ( );
		}

		public bool DdmSeen ( ) {
			throw new NotImplementedException ( );
		}

		public void Close ( bool notify ) {
			throw new NotImplementedException ( );
		}

		public void Update ( ClientChangeMask changeMask ) {
			throw new NotImplementedException ( );
		}

		public void Read ( ) {
			throw new NotImplementedException ( );
		}

		public object GetJdwpPacket ( ) {
			throw new NotImplementedException ( );
		}

		public void ForwardPacketToClient ( object packet ) {
			throw new NotImplementedException ( );
		}

		public bool SendHandshake ( ) {
			throw new NotImplementedException ( );
		}

		public void SendAndConsume ( object packet ) {
			throw new NotImplementedException ( );
		}


		public override String ToString ( ) {
			return String.Format ( "[Client pid: {0}]", ClientData.Pid );
		}
	}
}
