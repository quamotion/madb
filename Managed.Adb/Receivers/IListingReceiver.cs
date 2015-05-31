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
		/// <summary>
		/// Sets the children.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="children">The children.</param>
		void SetChildren ( FileEntry entry, FileEntry[] children );
		/// <summary>
		/// Refreshes the entry.
		/// </summary>
		/// <param name="entry">The entry.</param>
		void RefreshEntry ( FileEntry entry );
	}
}
