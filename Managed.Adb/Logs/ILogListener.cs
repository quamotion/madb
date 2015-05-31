using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Logs {
	public interface ILogListener {
		/// <summary>
		/// Sent when a new LogEntry has been parsed by the LogReceiver.
		/// </summary>
		/// <param name="entry">entry the new log entry.</param>
		void NewEntry ( LogEntry entry );
		/// <summary>
		/// Sent when new raw data is coming from the log service.
		/// </summary>
		/// <param name="data">the raw data buffer.</param>
		/// <param name="offset">the offset into the buffer signaling the beginning of the new data.</param>
		/// <param name="length">the length of the new data.</param>
		void NewData ( byte[] data, int offset, int length );
	}
}
