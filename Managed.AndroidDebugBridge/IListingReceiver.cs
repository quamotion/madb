using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// Classes which implement this interface provide a method that deals with asynchronous 
	/// result from <code>ls</code> command on the device.
	/// </summary>
	public interface IListingReceiver {
		void SetChildren ( FileEntry entry, FileEntry[] children );
		void RefreshEntry ( FileEntry entry );
	}
}
