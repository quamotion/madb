using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public interface ILogOutput {
		void Write ( LogLevel.LogLevelInfo logLevel, String tag, String message );
		void WriteAndPromptLog ( LogLevel.LogLevelInfo logLevel, String tag, String message );
	}

}
