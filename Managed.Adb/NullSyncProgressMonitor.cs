using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// A Sync progress monitor that does nothing
	/// </summary>
	public sealed class NullSyncProgressMonitor : ISyncProgressMonitor {

		/// <summary>
		/// Sent when the transfer starts
		/// </summary>
		/// <param name="totalWork">the total amount of work.</param>
		public void Start ( long totalWork ) {
		}

		/// <summary>
		/// Sent when the transfer is finished or interrupted.
		/// </summary>
		public void Stop ( ) {
		}

		/// <summary>
		/// Sent to query for possible cancellation.
		/// </summary>
		/// <returns><c>true</c> if the transfer should be stopped; otherwise, false</returns>
		public bool IsCanceled {
			get {
				return false;
			}
		}

		/// <summary>
		/// Sent when a sub task is started.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination">the destination.</param>
		public void StartSubTask ( String source, String destination ) {
		}

		/// <summary>
		/// Sent when some progress have been made.
		/// </summary>
		/// <param name="work">the amount of work done.</param>
		public void Advance ( long work ) {
		}
	}
}
