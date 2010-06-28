using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class ClientData {
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

		public enum AllocationTrackingStatus {
        /**
         * Allocation tracking status: unknown.
         * <p/>This happens right after a {@link Client} is discovered
         * by the {@link AndroidDebugBridge}, and before the {@link Client} answered the query
         * regarding its allocation tracking status.
         * @see Client#requestAllocationStatus()
         */
        UNKNOWN,
        /** Allocation tracking status: the {@link Client} is not tracking allocations. */
        OFF,
        /** Allocation tracking status: the {@link Client} is tracking allocations. */
        ON
    }

    public enum MethodProfilingStatus {
        /**
         * Method profiling status: unknown.
         * <p/>This happens right after a {@link Client} is discovered
         * by the {@link AndroidDebugBridge}, and before the {@link Client} answered the query
         * regarding its method profiling status.
         * @see Client#requestMethodProfilingStatus()
         */
        UNKNOWN,
        /** Method profiling status: the {@link Client} is not profiling method calls. */
        OFF,
        /** Method profiling status: the {@link Client} is profiling method calls. */
        ON
    }

		/**
     * Name of the value representing the max size of the heap, in the {@link Map} returned by
     * {@link #getVmHeapInfo(int)}
     */
    public const String HEAP_MAX_SIZE_BYTES = "maxSizeInBytes"; // $NON-NLS-1$
    /**
     * Name of the value representing the size of the heap, in the {@link Map} returned by
     * {@link #getVmHeapInfo(int)}
     */
    public const String HEAP_SIZE_BYTES = "sizeInBytes"; // $NON-NLS-1$
    /**
     * Name of the value representing the number of allocated bytes of the heap, in the
     * {@link Map} returned by {@link #getVmHeapInfo(int)}
     */
    public const String HEAP_BYTES_ALLOCATED = "bytesAllocated"; // $NON-NLS-1$
    /**
     * Name of the value representing the number of objects in the heap, in the {@link Map}
     * returned by {@link #getVmHeapInfo(int)}
     */
    public const String HEAP_OBJECTS_ALLOCATED = "objectsAllocated"; // $NON-NLS-1$

    /**
     * String for feature enabling starting/stopping method profiling
     * @see #hasFeature(String)
     */
    public const String FEATURE_PROFILING = "method-trace-profiling"; // $NON-NLS-1$

    /**
     * String for feature enabling direct streaming of method profiling data
     * @see #hasFeature(String)
     */
    public const String FEATURE_PROFILING_STREAMING = "method-trace-profiling-streaming"; // $NON-NLS-1$

    /**
     * String for feature allowing to dump hprof files
     * @see #hasFeature(String)
     */
    public const String FEATURE_HPROF = "hprof-heap-dump"; // $NON-NLS-1$

    /**
     * String for feature allowing direct streaming of hprof dumps
     * @see #hasFeature(String)
     */
    public const String FEATURE_HPROF_STREAMING = "hprof-heap-dump-streaming"; // $NON-NLS-1$

		public ClientData ( int pid ) {
			this.Pid = pid;

			DebuggerInterest = DebuggerStatus.DEFAULT;
			//ThreadMap = new Dictionary<Integer, ThreadInfo> ( );
		}

		public int Pid { get; private set; }
		public DebuggerStatus DebuggerInterest { get; set; }
		public bool IsDdmAware { get; set; }
		public String VmIdentifier { get; set; }
		public String Description { get; set; }
		public DebuggerStatus DebuggerConnectionStatus { get; set; }
	}
}
