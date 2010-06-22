using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class AdbResponse {

		public AdbResponse ( ) {
			Message = string.Empty;
		}

		public bool IOSuccess { get; set; }
		public bool Okay { get; set; }
		public bool Timeout { get; set; }
		public String Message { get; set; }
	}

}
