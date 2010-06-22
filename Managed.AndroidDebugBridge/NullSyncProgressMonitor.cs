using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// A Sync progress monitor that does nothing
	/// </summary>
	internal sealed class NullSyncProgressMonitor : ISyncProgressMonitor {

		public void Start ( long totalWork ) {
		}

		public void Stop ( ) {
		}

		public bool IsCanceled ( ) {
			return false;
		}

		public void StartSubTask ( string name ) {
		}

		public void Advance ( long work ) {
		}
	}
}
