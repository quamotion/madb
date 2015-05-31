using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// An Adb Communication Response
	/// </summary>
	public class AdbResponse {

		/// <summary>
		/// Initializes a new instance of the <see cref="AdbResponse"/> class.
		/// </summary>
		public AdbResponse ( ) {
			Message = string.Empty;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the IO communication was a success.
		/// </summary>
		/// <value>
		///   <c>true</c> if successful; otherwise, <c>false</c>.
		/// </value>
		public bool IOSuccess { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is okay.
		/// </summary>
		/// <value>
		///   <c>true</c> if okay; otherwise, <c>false</c>.
		/// </value>
		public bool Okay { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is timeout.
		/// </summary>
		/// <value>
		///   <c>true</c> if timeout; otherwise, <c>false</c>.
		/// </value>
		public bool Timeout { get; set; }
		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		/// <value>
		/// The message.
		/// </value>
		public String Message { get; set; }
	}

}
