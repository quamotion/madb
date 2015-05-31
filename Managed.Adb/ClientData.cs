using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public class ClientData {
		/// <summary>
		/// 
		/// </summary>
		private const String PRE_INITIALIZED = "<pre-initialized>";

		public enum DebuggerStatus {
			/// <summary>
			/// Debugger connection status: not waiting on one, not connected to one, but accepting
			/// new connections. This is the default value.
			/// </summary>
			DEFAULT,
			/// <summary>
			/// Debugger connection status: the application's VM is paused, waiting for a debugger to connect to it before resuming.
			/// </summary>
			WAITING,
			/// <summary>
			/// Debugger connection status : Debugger is connected
			/// </summary>
			ATTACHED,
			/// <summary>
			/// Debugger connection status: The listening port for debugger connection failed to listen.
			/// No debugger will be able to connect. 
			/// </summary>
			ERROR
		}

		/// <summary>
		/// 
		/// </summary>
		public enum AllocationTrackingStatus {
				/// <summary>
				/// Allocation tracking status: unknown.
				/// This happens right after a {@link Client} is discovered
				/// by the {@link AndroidDebugBridge}, and before the {@link Client} answered the query
				/// regarding its allocation tracking status.
				/// @see Client#requestAllocationStatus()
				/// </summary>
        UNKNOWN,
				/// <summary>
				/// Allocation tracking status: the {@link Client} is not tracking allocations.
				/// </summary>
        OFF,
				/// <summary>
				/// Allocation tracking status: the {@link Client} is tracking allocations.
				/// </summary>
        ON
    }

		/// <summary>
		/// 
		/// </summary>
    public enum MethodProfilingStatus {
				/// <summary>
				/// Method profiling status: unknown.
				/// This happens right after a {@link Client} is discovered
				/// by the {@link AndroidDebugBridge}, and before the {@link Client} answered the query
				/// regarding its method profiling status.
				/// @see Client#requestMethodProfilingStatus()
				/// </summary>
        UNKNOWN,
				/// <summary>
				///  Method profiling status: the {@link Client} is not profiling method calls.
				/// </summary>
        OFF,
				/// <summary>
				/// Method profiling status: the {@link Client} is profiling method calls.
				/// </summary>
        ON
    }

		/// <summary>
		/// Name of the value representing the max size of the heap, in the Map returned by
		/// GetVmHeapInfo(int)
		/// </summary>
    public const String HEAP_MAX_SIZE_BYTES = "maxSizeInBytes"; // $NON-NLS-1$
		/// <summary>
		/// Name of the value representing the size of the heap, in the {@link Map} returned by
		/// {@link #getVmHeapInfo(int)}
		/// </summary>
    public const String HEAP_SIZE_BYTES = "sizeInBytes"; // $NON-NLS-1$
		/// <summary>
		/// Name of the value representing the number of allocated bytes of the heap, in the
		/// {@link Map} returned by {@link #getVmHeapInfo(int)}
		/// </summary>
    public const String HEAP_BYTES_ALLOCATED = "bytesAllocated"; // $NON-NLS-1$
		/// <summary>
		/// Name of the value representing the number of objects in the heap, in the {@link Map}
		/// returned by {@link #getVmHeapInfo(int)}
		/// </summary>
    public const String HEAP_OBJECTS_ALLOCATED = "objectsAllocated"; // $NON-NLS-1$

		/// <summary>
		/// String for feature enabling starting/stopping method profiling
		/// @see #hasFeature(String)
		/// </summary>
    public const String FEATURE_PROFILING = "method-trace-profiling"; // $NON-NLS-1$

		/// <summary>
		/// String for feature enabling direct streaming of method profiling data
		/// @see #hasFeature(String)
		/// </summary>
    public const String FEATURE_PROFILING_STREAMING = "method-trace-profiling-streaming"; // $NON-NLS-1$

		/// <summary>
		/// String for feature allowing to dump hprof files
		/// @see #hasFeature(String)
		/// </summary>
    public const String FEATURE_HPROF = "hprof-heap-dump"; // $NON-NLS-1$

		/// <summary>
		/// String for feature allowing direct streaming of hprof dumps
		/// @see #hasFeature(String)		/// </summary>
    public const String FEATURE_HPROF_STREAMING = "hprof-heap-dump-streaming"; // $NON-NLS-1$

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientData"/> class.
		/// </summary>
		/// <param name="pid">The pid.</param>
		public ClientData ( int pid ) {
			this.Pid = pid;

			DebuggerInterest = DebuggerStatus.DEFAULT;
			//ThreadMap = new Dictionary<Integer, ThreadInfo> ( );
		}

		/// <summary>
		/// Gets the pid.
		/// </summary>
		public int Pid { get; private set; }
		/// <summary>
		/// Gets or sets the debugger interest.
		/// </summary>
		/// <value>
		/// The debugger interest.
		/// </value>
		public DebuggerStatus DebuggerInterest { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this instance is DDM aware.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is DDM aware; otherwise, <c>false</c>.
		/// </value>
		public bool IsDdmAware { get; set; }
		/// <summary>
		/// Gets or sets the vm identifier.
		/// </summary>
		/// <value>
		/// The vm identifier.
		/// </value>
		public String VmIdentifier { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		public String Description { get; set; }
		/// <summary>
		/// Gets or sets the debugger connection status.
		/// </summary>
		/// <value>
		/// The debugger connection status.
		/// </value>
		public DebuggerStatus DebuggerConnectionStatus { get; set; }
	}
}
