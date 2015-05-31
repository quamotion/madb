using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Managed.Adb {
		[Flags]
		public enum ClientChangeState {
			Name = 0x0001,
			DebuggerStatus = 0x0002,
			Port = 0x0004,
			ThreadMode = 0x0008,
			ThreadData = 0x0010,
			HeapMode = 0x0020,
			HeapData = 0x0040,
			NativeHeapData = 0x0080,
			ThreadStackTrace = 0x0100,
			HeapAllocations = 0x0200,
			HeapAllocationStatus = 0x0400,
			MethodProfilingStatus = 0x0800,
			Info = Name | DebuggerStatus | Port,
		}

		public enum ClientConnectionState {
			Init = 1,
			NotJDWP = 2,
			AwaitShake = 10,
			NeedDDMPacket = 11,
			NotDDM = 12,
			Ready = 13,
			Error = 20,
			Disconnected = 21,
		}

	public interface IClient : IPacketConsumer {


		ClientConnectionState ConnectionState { get; }
		ClientChangeState ChangeState { get; }
		Socket Channel { get; set; }

		IDevice Device { get; }
		Device DeviceImplementation { get; }
		int DebuggerListenPort { get; }
		bool IsDdmAware { get; }
		bool IsDebuggerAttached { get; }
		Debugger Debugger { get; }
		ClientData ClientData { get; }
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
