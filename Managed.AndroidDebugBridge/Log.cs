using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Managed.Adb.Extensions;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public sealed class Log {
		/// <summary>
		/// Gets or sets the level.
		/// </summary>
		/// <value>The level.</value>
		public static LogLevel.LogLevelInfo Level { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogOutput">LogOutput</see>
		/// </summary>
		public static ILogOutput LogOutput { get; set; }

		/// <summary>
		/// 
		/// </summary>
		private static char[] SpaceLine = new char[72];
		/// <summary>
		/// 
		/// </summary>
		private static readonly char[] HEXDIGIT = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

		/// <summary>
		/// Static Initializer for the <see cref="Log"/> class.
		/// </summary>
		static Log ( ) {
			/* prep for hex dump */
			int i = SpaceLine.Length - 1;
			while ( i >= 0 )
				SpaceLine[i--] = ' ';
			SpaceLine[0] = SpaceLine[1] = SpaceLine[2] = SpaceLine[3] = '0';
			SpaceLine[4] = '-';
			Level = DdmPreferences.LogLevel;
		}

		/// <summary>
		/// Cerates a new Instance of <see cref="Log"/>
		/// </summary>
		private Log ( ) {

		}

		/// <summary>
		/// 
		/// </summary>
		sealed class Config {
			/// <summary>
			/// Log Verbose
			/// </summary>
			public const bool LOGV = true;
			/// <summary>
			/// Log Debug
			/// </summary>
			public const bool LOGD = true;
		};

		/// <summary>
		/// Outputs a Verbose level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="message">The message to output</param>
		public static void v ( String tag, String message ) {
			WriteLine ( LogLevel.Verbose, tag, message );
		}

		/// <summary>
		/// Outputs a Verbose level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="format">The message to output format string.</param>
		/// <param name="args">The values for the format message</param>
		public static void v ( String tag, String format, params object[] args ) {
			WriteLine ( LogLevel.Verbose, tag, String.Format ( format, args ) );
		}

		/// <summary>
		/// Outputs a Debug level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="message">The message to output</param>
		public static void d ( String tag, String message ) {
			WriteLine ( LogLevel.Debug, tag, message );
		}

		/// <summary>
		/// Outputs a Debug level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="format">The message to output format string.</param>
		/// <param name="args">The values for the format message</param>
		public static void d ( String tag, String format, params object[] args ) {
			WriteLine ( LogLevel.Debug, tag, String.Format ( format, args ) );
		}

		/// <summary>
		/// Outputs a Info level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="format">The message to output.</param>
		public static void i ( String tag, String message ) {
			WriteLine ( LogLevel.Info, tag, message );
		}

		/// <summary>
		/// Outputs a Info level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="format">The message to output format string.</param>
		/// <param name="args">The values for the format message</param>
		public static void i ( String tag, String format, params object[] args ) {
			WriteLine ( LogLevel.Info, tag, String.Format ( format, args ) );
		}

		/// <summary>
		/// Outputs a Warn level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="message">The message to output.</param>
		public static void w ( String tag, String message ) {
			WriteLine ( LogLevel.Warn, tag, message );
		}

		/// <summary>
		/// Outputs a Warn level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="format">The message to output format string.</param>
		/// <param name="args">The values for the format message</param>
		public static void w ( String tag, String format, params object[] args ) {
			WriteLine ( LogLevel.Warn, tag, String.Format ( format, args ) );
		}

		/// <summary>
		/// Outputs a Warn level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="exception">The exception to warn</param>
		public static void w ( String tag, Exception exception ) {
			if ( exception != null ) {
				w ( tag, exception.ToString ( ) );
			}
		}

		/// <summary>
		/// Outputs a Warn level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="message">The message to output.</param>
		/// <param name="exception">The exception to warn</param>
		public static void w ( String tag, String message, Exception exception ) {
			w ( tag, "{0}\n{1}", message, exception );
		}

		/// <summary>
		/// Outputs a Error level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="message">The message to output.</param>
		public static void e ( String tag, String message ) {
			WriteLine ( LogLevel.Error, tag, message );
		}

		/// <summary>
		/// Outputs a Error level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="format">The message to output format string.</param>
		/// <param name="args">The values for the format message</param>
		public static void e ( String tag, String format, params object[] args ) {
			WriteLine ( LogLevel.Error, tag, String.Format ( format, args ) );
		}

		/// <summary>
		/// Outputs a Error level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="exception">The exception to warn</param>
		public static void e ( String tag, Exception exception ) {
			if ( exception != null ) {
				e ( tag, exception.ToString ( ) );
			}
		}

		/// <summary>
		/// Outputs a Error level message.
		/// </summary>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="message">The message to output.</param>
		/// <param name="exception">The exception to warn</param>
		public static void e ( String tag, String message, Exception exception ) {
			e ( tag, "{0}\n{1}", message, exception );
		}


		/// <summary>
		/// Outputs a log message and attempts to display it in a dialog.
		/// </summary>
		/// <param name="logLevel">The log level</param>
		/// <param name="tag">The tag associated with the message.</param>
		/// <param name="message">The message to output.</param>
		public static void LogAndDisplay ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			if ( LogOutput != null ) {
				LogOutput.WriteAndPromptLog ( logLevel, tag, message );
			} else {
				WriteLine ( logLevel, tag, message );
			}
		}


		/// <summary>
		/// Dump the entire contents of a byte array with DEBUG priority.
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="level"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <remarks>
		/// Local addition.  Output looks like:
		/// 1230- 00 11 22 33 44 55 66 77 88 99 aa bb cc dd ee ff  0123456789abcdef
		/// Uses no string concatenation; creates one String object per line.
		/// </remarks>
		internal static void HexDump ( String tag, LogLevel.LogLevelInfo level, byte[] data, int offset, int length ) {

			int kHexOffset = 6;
			int kAscOffset = 55;
			char[] line = new char[SpaceLine.Length];
			int addr, baseAddr, count;
			int i, ch;
			bool needErase = true;

			//Log.w(tag, "HEX DUMP: off=" + offset + ", length=" + length);

			baseAddr = 0;
			while ( length != 0 ) {
				if ( length > 16 ) {
					// full line
					count = 16;
				} else {
					// partial line; re-copy blanks to clear end
					count = length;
					needErase = true;
				}

				if ( needErase ) {
					Array.Copy ( SpaceLine, 0, line, 0, SpaceLine.Length );
					needErase = false;
				}

				// output the address (currently limited to 4 hex digits)
				addr = baseAddr;
				addr &= 0xffff;
				ch = 3;
				while ( addr != 0 ) {
					line[ch] = HEXDIGIT[addr & 0x0f];
					ch--;
					addr >>= 4;
				}

				// output hex digits and ASCII chars
				ch = kHexOffset;
				for ( i = 0; i < count; i++ ) {
					byte val = data[offset + i];

					line[ch++] = HEXDIGIT[( val >> 4 ) & 0x0f];
					line[ch++] = HEXDIGIT[val & 0x0f];
					ch++;

					if ( val >= 0x20 && val < 0x7f )
						line[kAscOffset + i] = (char)val;
					else
						line[kAscOffset + i] = '.';
				}

				WriteLine ( level, tag, new String ( line ) );

				// advance to next chunk of data
				length -= count;
				offset += count;
				baseAddr += count;
			}

		}


		/// <summary>
		/// Dump the entire contents of a byte array with DEBUG priority.
		/// </summary>
		/// <param name="data"></param>
		internal static void HexDump ( byte[] data ) {
			HexDump ( "ddms", LogLevel.Debug, data, 0, data.Length );
		}

		/// <summary>
		/// prints to stdout; could write to a log window
		/// </summary>
		/// <param name="logLevel"></param>
		/// <param name="tag"></param>
		/// <param name="message"></param>
		private static void WriteLine ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			if ( logLevel.Priority >= Level.Priority ) {
				if ( LogOutput != null ) {
					LogOutput.Write ( logLevel, tag, message );
				} else {
					Write ( logLevel, tag, message );
				}
			}
		}

		/// <summary>
		/// Prints a log message.
		/// </summary>
		/// <param name="logLevel"></param>
		/// <param name="tag"></param>
		/// <param name="message"></param>
		public static void Write ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			Console.Write ( GetLogFormatString ( logLevel, tag, message ) );
		}

		/// <summary>
		/// Formats a log message.
		/// </summary>
		/// <param name="logLevel"></param>
		/// <param name="tag"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static String GetLogFormatString ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			long msec = DateTime.Now.ToUnixEpoch ( );
			return String.Format ( "{0:00}:{1:00} {2}/{3}: {4}\n", ( msec / 60000 ) % 60, ( msec / 1000 ) % 60,
							logLevel.Letter, tag, message );
		}
	}
}
