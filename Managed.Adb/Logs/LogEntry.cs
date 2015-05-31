using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Logs {
	public sealed class LogEntry {
		public int Length { get; set; }
		public int ProcessId { get; set; }
		public int ThreadId { get; set; }
		public DateTime TimeStamp { get; set; }
		public int NanoSeconds { get; set; }
		public byte[] Data { get; set; }
	}
}
