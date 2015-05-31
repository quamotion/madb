using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public static class DdmPreferences {
		/** Default value for thread update flag upon client connection. */
		public const bool DEFAULT_INITIAL_THREAD_UPDATE = false;
		/** Default value for heap update flag upon client connection. */
		public const bool DEFAULT_INITIAL_HEAP_UPDATE = false;
		/** Default value for the selected client debug port */
		public const int DEFAULT_SELECTED_DEBUG_PORT = 8700;
		/** Default value for the debug port base */
		public const int DEFAULT_DEBUG_PORT_BASE = 8600;
		/** Default value for the logcat {@link LogLevel} */
		public static readonly LogLevel.LogLevelInfo DEFAULT_LOG_LEVEL = Managed.Adb.LogLevel.Error;
		/** Default timeout values for adb connection (milliseconds) */
		public const int DEFAULT_TIMEOUT = 5000; // standard delay, in ms

		private static int _selectedDebugPort;
		private static LogLevel.LogLevelInfo _logLevel;

		static DdmPreferences ( ) {
			Timeout = DEFAULT_TIMEOUT;
			LogLevel = DEFAULT_LOG_LEVEL;
			SelectedDebugPort = DEFAULT_SELECTED_DEBUG_PORT;
			DebugPortBase = DEFAULT_DEBUG_PORT_BASE;
			InitialThreadUpdate = DEFAULT_INITIAL_THREAD_UPDATE;
			InitialHeapUpdate = DEFAULT_INITIAL_HEAP_UPDATE;
		}

		public static int Timeout { get; set; }
		public static LogLevel.LogLevelInfo LogLevel {
			get {
				return _logLevel;
			}
			set {
				_logLevel = value;
				Log.Level = value ;
			}
		}
		public static int DebugPortBase { get; set; }
		public static int SelectedDebugPort {
			get {
				return _selectedDebugPort;
			}
			set {
				_selectedDebugPort = value;

				MonitorThread monitorThread = MonitorThread.Instance;
				if ( monitorThread != null ) {
					monitorThread.SetDebugSelectedPort ( value );
				}
			}
		}
		public static bool InitialThreadUpdate { get; set; }
		public static bool InitialHeapUpdate { get; set; }
	}
}
