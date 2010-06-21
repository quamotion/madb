using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public interface IShellOutputReceiver {
		void AddOutput ( byte[] data, int offset, int length );
		void Flush ( );
		bool IsCancelled { get; }
	}

}
