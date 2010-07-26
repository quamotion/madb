using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO {
	public static class PathHelper {

		public static String Combine ( params String[] paths ) {
			String lastPath = String.Empty;

			foreach ( var item in paths ) {
				lastPath = Path.Combine ( lastPath, item );
			}

			return lastPath;
		}

	}
}
