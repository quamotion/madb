using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public interface ILogOutput {
		/// <summary>
		/// Writes the specified log level.
		/// </summary>
		/// <param name="logLevel">The log level.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="message">The message.</param>
		void Write ( LogLevel.LogLevelInfo logLevel, String tag, String message );
		/// <summary>
		/// Writes the and prompt log.
		/// </summary>
		/// <param name="logLevel">The log level.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="message">The message.</param>
		void WriteAndPromptLog ( LogLevel.LogLevelInfo logLevel, String tag, String message );
	}

}
