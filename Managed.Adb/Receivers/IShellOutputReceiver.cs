using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public interface IShellOutputReceiver {
		/// <summary>
		/// Adds the output.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="length">The length.</param>
		void AddOutput ( byte[] data, int offset, int length );
		/// <summary>
		/// Flushes the output. 
		/// </summary>
		/// <remarks>This should always be called at the end of the "process" in order to indicate that the data is ready to be processed further if needed.</remarks>
		void Flush ( );
		/// <summary>
		/// Gets a value indicating whether this instance is cancelled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
		/// </value>
		bool IsCancelled { get; }
	}

}
