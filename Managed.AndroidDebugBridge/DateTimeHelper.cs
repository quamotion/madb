using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public static class DateTimeHelper {
		private static readonly DateTime Epoch = new DateTime ( 1970, 1, 1 );

		public static long CurrentMillis (  ) {
			return (long)( DateTime.UtcNow - Epoch ).TotalMilliseconds;
		}
	}
}
