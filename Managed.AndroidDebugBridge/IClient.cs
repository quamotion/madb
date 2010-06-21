using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public interface IClient : IPacketConsumer {
		IDevice Device { get; }
		Device DeviceImplementation { get; }
		int DebuggerListenPort { get; }
		bool IsDdmAware { get; }
		bool IsDebuggerAttached { get; }
		Debugger Debugger { get; }
		// TODO: ClientData
		/*ClientData*/ Object ClientData { get; }
		bool IsThreadUpdateEnabled { get; set; }
		bool IsHeapUpdateEnabled { get; set; }
		bool IsSelectedClient { get; set; }
		bool IsValid { get; }

		void ExecuteGarbageCollector ( );
		void DumpHprof ( );
		void ToggleMethodProfiling ( );
		void RequestMethodProfilingStatus ( );
		void RequestThreadUpdate ( );
		void RequestThreadStackTrace ( int threadID );
		bool RequestNativeHeapInformation ( );
		void EnableAllocationTracker ( bool enable );
		void RequestAllocationStatus ( );
		void RequestAllocationDetails ( );
		void Kill ( );
		// TODO: Define Selector
		void Register ( /*Selector*/ Object selector );
		void ListenForDebugger ( int listenPort );
		// TODO: JdwpPacket
		void SendAndConsume ( /*JdwpPacket*/ Object packet, ChunkHandler replyHandler );
		void AddRequestId ( int id, ChunkHandler handler );
		void RemoveRequestId ( int id );
		ChunkHandler IsResponseToUs ( int id );
		// TODO: JdwpPacket
		void PacketFailed ( /*JdwpPacket*/ Object packet );
		bool DdmSeen ( );
		void Close ( bool notify );
		void Update ( ClientChangeMask changeMask );
	}
}
