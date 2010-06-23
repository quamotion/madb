using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// A Sync progress monitor that does nothing
	/// </summary>
	public sealed class NullSyncProgressMonitor : ISyncProgressMonitor {

		public void Start ( long totalWork ) {
		}

		public void Stop ( ) {
		}

		public bool IsCanceled {
			get {
				return false;
			}
		}

		public void StartSubTask ( string name ) {
		}

		public void Advance ( long work ) {
		}
	}
}
