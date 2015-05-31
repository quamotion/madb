using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// Classes which implement this interface provide methods that deal with displaying transfer progress.
	/// </summary>
	public interface ISyncProgressMonitor {
		/// <summary>
		/// Sent when the transfer starts
		/// </summary>
		/// <param name="totalWork">the total amount of work.</param>
		void Start ( long totalWork );

		/// <summary>
		/// Sent when the transfer is finished or interrupted.
		/// </summary>
		void Stop ( );

		/// <summary>
		/// Sent to query for possible cancellation.
		/// </summary>
		/// <returns><c>true</c> if the transfer should be stopped; otherwise, false</returns>
		bool IsCanceled { get; }

		/// <summary>
		/// Sent when a sub task is started.
		/// </summary>
		/// <param name="destination">the destination.</param>
		void StartSubTask ( String source, String destination );

		/// <summary>
		/// Sent when some progress have been made.
		/// </summary>
		/// <param name="work">the amount of work done.</param>
		void Advance ( long work );
	}
}
